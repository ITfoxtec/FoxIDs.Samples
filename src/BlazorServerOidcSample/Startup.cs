using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorServerOidcSample.Models;
using BlazorServerOidcSample.Data;
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
using Microsoft.Extensions.Hosting;
using ITfoxtec.Identity.Util;
using BlazorServerOidcSample.Identity;
using FoxIDs.SampleHelperLibrary.Identity;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorServerOidcSample
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

            services.AddSingleton((serviceProvider) =>
            {
                var settings = serviceProvider.GetService<IdentitySettings>();
                var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

                return new OidcDiscoveryHandler(httpClientFactory, UrlCombine.Combine(settings.AuthorityWithoutUpParty, IdentityConstants.OidcDiscovery.Path));
            });

            services.AddScoped<AuthenticationStateProvider, AuthenticationStateValidator>();

            services.AddSingleton<LogoutMemoryCache>();
            services.AddScoped<TokenProvider>();
            services.AddSingleton<WeatherForecastService>();

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddHttpContextAccessor();
            services.AddHttpClient();

            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(options => 
                {
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

                    options.MapInboundClaims = false;
                    options.TokenValidationParameters.NameClaimType = JwtClaimTypes.Subject;
                    options.TokenValidationParameters.RoleClaimType = JwtClaimTypes.Role;

                    options.Events.OnTokenValidated = async (context) =>
                    {
                        var logoutMemoryCache = context.HttpContext.RequestServices.GetService<LogoutMemoryCache>();
                        logoutMemoryCache.Clear();
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
                        if (context.Failure != null)
                        {
                            throw new Exception("Remote failure.", context.Failure);
                        }
                        await Task.FromResult(string.Empty);
                    };
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!env.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
