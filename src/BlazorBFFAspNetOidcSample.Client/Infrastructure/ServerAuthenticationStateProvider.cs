using BlazorBFFAspNetOidcSample.Models.Api;
using ITfoxtec.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazorBFFAspNetOidcSample.Client.Infrastructure
{
    public class ServerAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IHttpClientFactory httpClientFactory;

        public ServerAuthenticationStateProvider(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            //var claims = new List<Claim>();
            //claims.Add(new Claim(JwtClaimTypes.Subject, "testuser1"));

            //var identity = new ClaimsIdentity(claims, nameof(ServerAuthenticationStateProvider), JwtClaimTypes.Subject, JwtClaimTypes.Role);
            var identity = await GetIdentityAsync();
            return new AuthenticationState(identity != null ? new ClaimsPrincipal(identity) : new ClaimsPrincipal());
        }

        private async Task<ClaimsIdentity> GetIdentityAsync()
        {
            var apiUrl = "api/identity";
            using var response = await httpClientFactory.CreateClient("server.secure").GetAsync(apiUrl);
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new Exception($"Error, Bad request. StatusCode={response.StatusCode}, API URL='{apiUrl}'");
            }
            var identityResponse = await response.Content.ReadFromJsonAsync<IdentityResponse>();

            if (identityResponse.IsAuthenticated)
            {
                var identity = new ClaimsIdentity(nameof(ServerAuthenticationStateProvider), JwtClaimTypes.Subject, JwtClaimTypes.Role);
                foreach (var claim in identityResponse.Claims)
                {
                    identity.AddClaim(new Claim(claim.Type, claim.Value));
                }
                return identity;
            }
            else
            {
                return null;
            }
        }
    }
}
