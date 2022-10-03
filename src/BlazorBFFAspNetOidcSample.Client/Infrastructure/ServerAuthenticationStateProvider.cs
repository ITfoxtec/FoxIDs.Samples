using BlazorBFFAspNetOidcSample.Models.Api;
using ITfoxtec.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorBFFAspNetOidcSample.Client.Infrastructure
{
    public class ServerAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<ServerAuthenticationStateProvider> logger;

        public ServerAuthenticationStateProvider(IHttpClientFactory httpClientFactory, ILogger<ServerAuthenticationStateProvider> logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = await GetIdentityAsync();

            if(identity.IsAuthenticated)
            {
                logger.LogInformation("User logged in.");

                Timer timer = null;
                timer = new Timer(async _ =>
                {
                    var identity = await GetIdentityAsync();
                    if (!identity.IsAuthenticated)
                    {
                        logger.LogInformation("User logged out.");
                        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));
                        await timer.DisposeAsync();
                    }
                }, null, 30000, 10000);
            }

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        private async Task<ClaimsIdentity> GetIdentityAsync()
        {
            var apiUrl = "api/identity";
            using var response = await httpClientFactory.CreateClient(Constants.Client.HttpClientSecureLogicalName).GetAsync(apiUrl);
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
                return new ClaimsIdentity();
            }
        }
    }
}
