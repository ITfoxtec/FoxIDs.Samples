using ITfoxtec.Identity.Discovery;
using ITfoxtec.Identity.Messages;
using ITfoxtec.Identity.Tokens;
using ITfoxtec.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Net;
using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;
using BlazorServerOidcSample.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System;
using System.Linq;

namespace BlazorServerOidcSample.Identity
{
    public static class RefreshTokenHandler
    {
        public static async Task<TokenResponse> ResolveRefreshToken(CookieValidatePrincipalContext context, IdentitySettings identitySettings)
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
            using var response = await client.SendAsync(request);
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
                    var atHash = newPrincipal.Claims.Where(c => c.Type == JwtClaimTypes.AtHash).Single().Value;
                    if (atHash != await tokenResponse.AccessToken.LeftMostBase64urlEncodedHashAsync(IdentityConstants.Algorithms.Asymmetric.RS256))
                    {
                        throw new Exception("Access Token hash claim in ID token do not match the access token.");
                    }
                    if (context.Principal.Claims.Where(c => c.Type == JwtClaimTypes.Subject).Single().Value != newPrincipal.Claims.Where(c => c.Type == JwtClaimTypes.Subject).Single().Value)
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
    }
}
