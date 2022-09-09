﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BlazorServerOidcSample.Models;
using BlazorServerOidcSample.Data;
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
using FoxIDs.SampleHelperLibrary.Models;
using FoxIDs.SampleHelperLibrary.Repository;
using Microsoft.Extensions.Hosting;
using UrlCombineLib;
using Microsoft.AspNetCore.Builder;

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

            services.AddTransient<IdPSelectionCookieRepository>();
            services.AddSingleton((serviceProvider) =>
            {
                var settings = serviceProvider.GetService<IdentitySettings>();
                var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

                return new OidcDiscoveryHandler(httpClientFactory, UrlCombine.Combine(settings.Authority, IdentityConstants.OidcDiscovery.Path));
            });

            services.AddScoped<TokenProvider>();
            services.AddSingleton<WeatherForecastService>();

            services.AddRazorPages();
            services.AddServerSideBlazor();
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