using BlazorBFFAspNetOidcSample.Models.Api;
using ITfoxtec.Identity;
using System.Net;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Blazored.SessionStorage;

namespace BlazorBFFAspNetOidcSample.Client.Infrastructure
{
    public class AntiforgeryHandler : DelegatingHandler
    {
        private const string antiforgeryTokenStoreKey = "antiforgery-token";
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ISessionStorageService sessionStorage;
        private string antiforgeryToken;

        public AntiforgeryHandler(IHttpClientFactory httpClientFactory, ISessionStorageService sessionStorage)
        {
            this.httpClientFactory = httpClientFactory;
            this.sessionStorage = sessionStorage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Add the first antiforgery token, the call is secured with the Antiforgery cookie and not the token.
            request.Headers.Add("X-CSRF-TOKEN", await GetAntiforgeryTokenAsync());

            return await base.SendAsync(request, cancellationToken);
        }

        private async Task<string> GetAntiforgeryTokenAsync()
        {
           // var antiforgeryToken = await sessionStorage.GetItemAsync<string>(antiforgeryTokenStoreKey);

            if (antiforgeryToken.IsNullOrEmpty())
            {
                var apiUrl = "api/antiforgerytoken";
                using var response = await httpClientFactory.CreateClient("server").GetAsync(apiUrl);
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new Exception($"Error, Bad request. StatusCode={response.StatusCode}, API URL='{apiUrl}'");
                }
                var antiforgeryTokenResponse = await response.Content.ReadFromJsonAsync<AntiforgeryTokenResponse>();
                antiforgeryToken = antiforgeryTokenResponse.Token;
                //await sessionStorage.SetItemAsync(antiforgeryTokenStoreKey, antiforgeryToken);
            }

            return antiforgeryToken;
        }
    }
}
