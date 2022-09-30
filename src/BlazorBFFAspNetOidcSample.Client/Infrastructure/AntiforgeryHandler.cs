using BlazorBFFAspNetOidcSample.Models.Api;
using ITfoxtec.Identity;
using System.Net;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorBFFAspNetOidcSample.Client.Infrastructure
{
    public class AntiforgeryHandler : DelegatingHandler
    {
        private readonly IHttpClientFactory httpClientFactory;
        private string antiforgeryToken;

        public AntiforgeryHandler(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Add the first antiforgery token, the call is secured with the Antiforgery cookie and not the token.
            request.Headers.Add(Constants.AntiforgeryTokenHeaderName, await GetAntiforgeryTokenAsync());

            return await base.SendAsync(request, cancellationToken);
        }

        public async Task<string> GetAntiforgeryTokenAsync()
        {
            if (antiforgeryToken.IsNullOrEmpty())
            {
                var apiUrl = "api/antiforgerytoken";
                using var response = await httpClientFactory.CreateClient(Constants.Client.HttpClientLogicalName).GetAsync(apiUrl);
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new Exception($"Error, Bad request. StatusCode={response.StatusCode}, API URL='{apiUrl}'");
                }
                var antiforgeryTokenResponse = await response.Content.ReadFromJsonAsync<AntiforgeryTokenResponse>();
                antiforgeryToken = antiforgeryTokenResponse.Token;
            }

            return antiforgeryToken;
        }
    }
}
