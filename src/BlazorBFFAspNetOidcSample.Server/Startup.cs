using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorBFFAspNetOidcSample.Server.Models;
using ITfoxtec.Identity;
using ITfoxtec.Identity.Discovery;
using ITfoxtec.Identity.Messages;
using ITfoxtec.Identity.Tokens;
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
using BlazorBFFAspNetOidcSample.Server.Infrastructur.Proxy;
using BlazorBFFAspNetOidcSample.Server.Infrastructur;

namespace BlazorBFFAspNetOidcSample.Server
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
          
            var identitySettings = services.BindConfig<IdentitySettings>(Configuration, nameof(IdentitySettings));
            services.BindConfig<AppSettings>(Configuration, nameof(AppSettings));

            services.AddSingleton((serviceProvider) =>
            {
                var settings = serviceProvider.GetService<IdentitySettings>();
                var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

                return new OidcDiscoveryHandler(httpClientFactory, UrlCombine.Combine(settings.Authority, IdentityConstants.OidcDiscovery.Path));
            });
            services.AddControllersWithViews();
            services.AddHttpContextAccessor();
            services.AddHttpClient();
            services.AddReverseProxy();
            services.AddTransient<ApiTransformBuilder>();

            services.AddAntiforgery(options =>
            {
                options.HeaderName = Constants.AntiforgeryTokenHeaderName;
            });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(options => 
                {
                    // Required at a lover level to support Front channel logout
                    options.Cookie.SameSite = SameSiteMode.None;
                    // but it is more secure to use
                    //options.Cookie.SameSite = SameSiteMode.Strict;

                    options.Events.OnValidatePrincipal = async (context) =>
                    {
                        try
                        {
                            var expiresUtc = DateTimeOffset.Parse(context.Properties.GetTokenValue("expires_at"));

                            // Tokens expires 30 seconds before actual expiration time.
                            if (expiresUtc < DateTimeOffset.UtcNow.AddSeconds(30))
                            {
                                var tokenResponse = await RefreshTokens(context, identitySettings);

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
                    options.Authority = identitySettings.Authority;
                    options.ClientId = identitySettings.ClientId;
                    options.ClientSecret = identitySettings.ClientSecret;

                    options.ResponseType = OpenIdConnectResponseType.Code;

                    options.SaveTokens = true;

                    // Scope to the application it self.
                    //options.Scope.Add("blazor_bff_aspnetcore_oidc_sample");
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
                    //options.Events.OnRemoteFailure = async (context) =>
                    //{
                    //    await Task.FromResult(string.Empty);
                    //};
                });
        }

        private async Task<TokenResponse> RefreshTokens(CookieValidatePrincipalContext context, IdentitySettings identitySettings)
        {
            var tokenRequest = new TokenRequest
            {
                GrantType = IdentityConstants.GrantTypes.RefreshToken,
                RefreshToken = context.Properties.GetTokenValue(OpenIdConnectParameterNames.RefreshToken),
                ClientId = identitySettings.ClientId,
            };
            var clientCredentials = new ClientCredentials
            {
                ClientSecret = identitySettings.ClientSecret,
            };

            var oidcDiscoveryHandler = context.HttpContext.RequestServices.GetService<OidcDiscoveryHandler>();
            var oidcDiscovery = await oidcDiscoveryHandler.GetOidcDiscoveryAsync();

            var request = new HttpRequestMessage(HttpMethod.Post, oidcDiscovery.TokenEndpoint);
            request.Content = new FormUrlEncodedContent(tokenRequest.ToDictionary().AddToDictionary(clientCredentials));

            var httpClientFactory = context.HttpContext.RequestServices.GetService<IHttpClientFactory>();

            var client = httpClientFactory.CreateClient();
            var response = await client.SendAsync(request);
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    var result = await response.Content.ReadAsStringAsync();
                    var tokenResponse = result.ToObject<TokenResponse>();
                    tokenResponse.Validate(true);
                    if (tokenResponse.AccessToken.IsNullOrEmpty()) throw new ArgumentNullException(nameof(tokenResponse.AccessToken), tokenResponse.GetTypeName());
                    if (tokenResponse.ExpiresIn <= 0) throw new ArgumentNullException(nameof(tokenResponse.ExpiresIn), tokenResponse.GetTypeName());

                    var oidcDiscoveryKeySet = await oidcDiscoveryHandler.GetOidcDiscoveryKeysAsync();
                    (var newPrincipal, var newSecurityToken) = JwtHandler.ValidateToken(tokenResponse.IdToken, oidcDiscovery.Issuer, oidcDiscoveryKeySet.Keys, identitySettings.ClientId);
                    if(context.Principal.Claims.Where(c => c.Type == JwtClaimTypes.Subject).Single().Value != newPrincipal.Claims.Where(c => c.Type == JwtClaimTypes.Subject).Single().Value)
                    {
                        throw new Exception("New principal has invalid sub claim.");
                    }

                    return tokenResponse;

                case HttpStatusCode.BadRequest:
                    var resultBadRequest = await response.Content.ReadAsStringAsync();
                    var tokenResponseBadRequest = resultBadRequest.ToObject<TokenResponse>();
                    tokenResponseBadRequest.Validate(true);
                    throw new Exception($"Error, Bad request. StatusCode={response.StatusCode}");

                default:
                    throw new Exception($"Error, Status Code not expected. StatusCode={response.StatusCode}");
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AppSettings appSettings)
        {
            if (env.IsDevelopment())
            {
                //app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.AddApiProxy("/proxy/api1", appSettings.AspNetCoreApi1SampleUrl, proxyTimeoutInSeconds: 100);

                endpoints.MapControllerRoute(
                   name: "default",
                   pattern: "{controller}/{action}/{id?}");

                endpoints.MapFallbackToFile("index.html");
            });


        }
    }
}
