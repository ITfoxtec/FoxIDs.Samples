using System;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using AspNetCoreApi1Sample.Policys;
using ITfoxtec.Identity;
using ITfoxtec.Identity.Discovery;
using ITfoxtec.Identity.Helpers;
using ITfoxtec.Identity.Messages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using AspNetCoreApi1Sample.Models;
using System.Net.Http;
using ITfoxtec.Identity.Util;

namespace AspNetCoreApi1Sample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Api1SomeAccessScopeAuthorize]
    public class ValuesApi2Controller : ControllerBase
    {
        private readonly IdentitySettings identitySettings;
        private readonly AppSettings appSettings;
        private readonly TokenExecuteHelper tokenExecuteHelper;
        private readonly OidcDiscoveryHandler oidcDiscoveryHandler;
        private readonly IHttpClientFactory httpClientFactory;

        public ValuesApi2Controller(IdentitySettings identitySettings, AppSettings appSettings, TokenExecuteHelper tokenExecuteHelper, OidcDiscoveryHandler oidcDiscoveryHandler, IHttpClientFactory httpClientFactory)
        {
            this.identitySettings = identitySettings;
            this.appSettings = appSettings;
            this.tokenExecuteHelper = tokenExecuteHelper;
            this.oidcDiscoveryHandler = oidcDiscoveryHandler;
            this.httpClientFactory = httpClientFactory;
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<string>> Get()
        {
            // Call API2
            var accessTokenApi2 = await GetTokenForApi2();
            var apiUrl = appSettings.AspNetCoreApi2SampleUrl;
            using var response = await httpClientFactory.CreateClient().GetAsync(apiUrl, accessTokenApi2, "4321");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception($"Unable to call API2. API URL='{apiUrl}', StatusCode='{response.StatusCode}'.");
            }
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<ActionResult<string>> Get(int id)
        {
            // Call API2
            var accessTokenApi2 = await GetTokenForApi2();
            var apiUrl = appSettings.AspNetCoreApi2SampleUrl;
            using var response = await httpClientFactory.CreateClient().GetAsync(apiUrl, accessTokenApi2, Convert.ToString(id));
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception($"Unable to call API2. API URL='{apiUrl}', StatusCode='{response.StatusCode}', API2 AccessToken '{accessTokenApi2}'.");
            }
        }

        // TokenExchange for API2
        private async Task<string> GetTokenForApi2()
        {
            var accessToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            var oidcDiscovery = await oidcDiscoveryHandler.GetOidcDiscoveryAsync();

            var tokenExchangeRequest = new TokenExchangeRequest
            {
                Scope = "aspnetcore_api2_sample:some_2_access",
                SubjectToken = accessToken,
                SubjectTokenType = IdentityConstants.TokenTypeIdentifiers.AccessToken
            };

            var tokenExchangeResponse = await tokenExecuteHelper.ExecuteTokenRequestWithAssertionClientCredentialGrantAsync<TokenExchangeRequest, TokenExchangeResponse>(GetClientCertificate(), identitySettings.ClientId, tokenEndpoint: oidcDiscovery.TokenEndpoint, tokenRequest: tokenExchangeRequest);

            return tokenExchangeResponse.AccessToken;
        }

        private X509Certificate2 GetClientCertificate()
        {
            if (!identitySettings.TokenExchangeClientCertificateThumbprint.IsNullOrEmpty())
            {
                return CertificateUtil.Load(StoreName.My, StoreLocation.CurrentUser, X509FindType.FindByThumbprint, identitySettings.TokenExchangeClientCertificateThumbprint);
            }
            else
            {
                return CertificateUtil.Load(Path.Combine(Startup.AppEnvironment.ContentRootPath, identitySettings.TokenExchangeClientCertificateFile), identitySettings.TokenExchangeClientCertificatePassword);
            }
        }
    }
}
