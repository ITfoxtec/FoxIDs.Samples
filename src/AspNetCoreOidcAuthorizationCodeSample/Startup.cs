using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Threading.Tasks;
using AspNetCoreOidcAuthorizationCodeSample.Models;
using ITfoxtec.Identity;
using ITfoxtec.Identity.Discovery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using FoxIDs.SampleHelperLibrary.Models;
using FoxIDs.SampleHelperLibrary.Repository;
using Microsoft.Extensions.Hosting;
using UrlCombineLib;
using AspNetCoreOidcAuthorizationCodeSample.Identity;

namespace AspNetCoreOidcAuthorizationCodeSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            IdentityModelEventSource.ShowPII = true; //To show detail of error and see the problem

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            
            var identitySettings = services.BindConfig<IdentitySettings>(Configuration, nameof(IdentitySettings));
            services.BindConfig<AppSettings>(Configuration, nameof(AppSettings));

            services.AddTransient<IdPSelectionCookieRepository>();
            services.AddSingleton((serviceProvider) =>
            {
                var settings = serviceProvider.GetService<IdentitySettings>();
                var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

                return new OidcDiscoveryHandler(httpClientFactory, UrlCombine.Combine(settings.AuthorityWithoutUpParty, IdentityConstants.OidcDiscovery.Path));
            });
            services.AddControllersWithViews();
            services.AddHttpContextAccessor();
            services.AddHttpClient();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(options => 
                {
                    // Required to support Front channel logout
                    options.Cookie.SameSite = SameSiteMode.None;

                    options.Events.OnValidatePrincipal = async (context) =>
                    {
                        try
                        {
                            var expiresUtc = DateTimeOffset.Parse(context.Properties.GetTokenValue("expires_at"));

                            // Tokens expires 30 seconds before actual expiration time.
                            if (expiresUtc < DateTimeOffset.UtcNow.AddSeconds(30))
                            {
                                var tokenResponse = await RefreshTokenHandler.ResolveRefreshToken(context, identitySettings);

                                context.Properties.UpdateTokenValue(OpenIdConnectParameterNames.AccessToken, tokenResponse.AccessToken);
                                context.Properties.UpdateTokenValue(OpenIdConnectParameterNames.IdToken, tokenResponse.IdToken);
                                if (!tokenResponse.RefreshToken.IsNullOrEmpty())
                                {
                                    context.Properties.UpdateTokenValue(OpenIdConnectParameterNames.RefreshToken, tokenResponse.RefreshToken);
                                }
                                else
                                {
                                    context.Properties.UpdateTokenValue(OpenIdConnectParameterNames.RefreshToken, context.Properties.GetTokenValue(OpenIdConnectParameterNames.RefreshToken));
                                }
                                context.Properties.UpdateTokenValue(OpenIdConnectParameterNames.TokenType, tokenResponse.TokenType);

                                var newExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn.HasValue ? tokenResponse.ExpiresIn.Value : 30);
                                context.Properties.UpdateTokenValue("expires_at", newExpiresUtc.ToString("o", CultureInfo.InvariantCulture));

                                // Cookie should be renewed.
                                context.ShouldRenew = true;
                            }
                        }
                        catch
                        {
                            context.RejectPrincipal();
                            await context.HttpContext.SignOutAsync();
                        }
                    };
                })
                .AddOpenIdConnect(options =>
                {
                    options.Authority = identitySettings.AuthorityWithoutUpParty;
                    options.ClientId = identitySettings.ClientId;
                    options.ClientSecret = identitySettings.ClientSecret;

                    options.ResponseType = OpenIdConnectResponseType.Code;

                    options.SaveTokens = true;
                    // False to support refresh token renewal.
                    options.UseTokenLifetime = false;

                    // Scope to the application it self.
                    //options.Scope.Add("aspnetcore_oidcauthcode_sample");
                    options.Scope.Add("aspnetcore_api1_sample:some_access");
                    options.Scope.Add("offline_access");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");

                    options.TokenValidationParameters.NameClaimType = JwtClaimTypes.Subject;
                    options.TokenValidationParameters.RoleClaimType = JwtClaimTypes.Role;

                    options.Events.OnRedirectToIdentityProvider = async (context) =>
                    {
                        // To require MFA
                        //context.ProtocolMessage.AcrValues = "urn:foxids:mfa";

                        var loginType = ReadLoginType(context.Properties);
                        SetIssuerAddress(context, loginType);

                        await Task.FromResult(string.Empty);
                    };
                    // FoxIDs support OIDC RP Initiated Logout without up-party in the URL if the ID Token is provided in the request.
                    //options.Events.OnRedirectToIdentityProviderForSignOut = async (context) =>
                    //{
                    //    var loginType = await GetSelectedLoginType(context.HttpContext);
                    //    SetIssuerAddress(context, loginType);

                    //    await Task.FromResult(string.Empty);
                    //};
                    options.Events.OnTokenValidated = async (context) =>
                    {
                        var idPSelectionCookieRepository = context.HttpContext.RequestServices.GetService<IdPSelectionCookieRepository>();
                        var loginType = ReadLoginType(context.Properties);
                        await idPSelectionCookieRepository.SaveAsync(loginType.ToString());

                        await Task.FromResult(string.Empty);
                    };
                    options.Events.OnTokenResponseReceived = async (context) =>
                    {
                        if(!context.TokenEndpointResponse.Error.IsNullOrEmpty())
                        {
                            throw new Exception($"Token response error. {context.TokenEndpointResponse.Error}, {context.TokenEndpointResponse.ErrorDescription} ");
                        }
                        await Task.FromResult(string.Empty);
                    };
                    options.Events.OnRemoteFailure = async (context) =>
                    {
                        await Task.FromResult(string.Empty);
                    };
                });
        }

        private static LoginType ReadLoginType(AuthenticationProperties properties)
        {
            LoginType loginType;
            if (!properties.Items.ContainsKey(Constants.StateLoginType) || !Enum.TryParse(properties.Items[Constants.StateLoginType], true, out loginType))
            {
                loginType = LoginType.FoxIDsLogin;
            }

            return loginType;
        }

        private async Task<LoginType> GetSelectedLoginType(HttpContext httpContext)
        {
            var idPSelectionCookieRepository = httpContext.RequestServices.GetService<IdPSelectionCookieRepository>();

            var loginTypeValue = await idPSelectionCookieRepository.GetAsync();
            if (!string.IsNullOrEmpty(loginTypeValue))
            {
                LoginType loginType;
                if (Enum.TryParse(loginTypeValue, true, out loginType))
                {
                    return loginType;
                }
            }

            throw new InvalidOperationException("Unable to read Login Type from IdP session cookie.");
        }

        private void SetIssuerAddress(RedirectContext context, LoginType loginType)
        {
            if (context.ProtocolMessage.RequestType != OpenIdConnectRequestType.Token)
            {
                var settings = context.HttpContext.RequestServices.GetService<IdentitySettings>();

                var upParty = GetUpParty(settings, loginType);
                context.ProtocolMessage.IssuerAddress = context.ProtocolMessage.IssuerAddress.Replace($"/{settings.DownParty}/", $"/{settings.DownParty}({upParty})/");
            }
        }

        private string GetUpParty(IdentitySettings settings, LoginType loginType)
        {
            switch (loginType)
            {
                case LoginType.FoxIDsLogin:
                    return settings.FoxIDsLoginUpParty;
                case LoginType.ParallelFoxIDs:
                    return settings.ParallelFoxIDsUpParty;
                case LoginType.IdentityServer:
                    return settings.IdentityServerUpParty;
                case LoginType.AzureAd:
                    return settings.AzureAdUpParty;
                case LoginType.SamlIdPSample:
                    return settings.SamlIdPSampleUpParty;
                case LoginType.SamlAdfs:
                    return settings.SamlAdfsUpParty;
                case LoginType.SamlNemLogin:
                    return settings.SamlNemLoginUpParty;
                default:
                    throw new NotImplementedException("LoginType not implemented.");
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
