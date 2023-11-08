using AspNetCoreOidcAuthCodeAllUpPartiesSample.Models;
using ITfoxtec.Identity;
using ITfoxtec.Identity.Discovery;
using ITfoxtec.Identity.Helpers;
using ITfoxtec.Identity.Messages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace AspNetCoreOidcAuthCodeAllUpPartiesSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppSettings appSettings;
        private readonly IdentitySettings identitySettings;
        private readonly TokenExecuteHelper tokenExecuteHelper;
        private readonly OidcDiscoveryHandler oidcDiscoveryHandler;
        private readonly IHttpClientFactory httpClientFactory;

        public HomeController(AppSettings appSettings, IdentitySettings identitySettings, TokenExecuteHelper tokenExecuteHelper, OidcDiscoveryHandler oidcDiscoveryHandler, IHttpClientFactory httpClientFactory)
        {
            this.appSettings = appSettings;
            this.identitySettings = identitySettings;
            this.tokenExecuteHelper = tokenExecuteHelper;
            this.oidcDiscoveryHandler = oidcDiscoveryHandler;
            this.httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Secure()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> CallApi1()
        {
            var accessToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            var apiUrl = appSettings.AspNetCoreApi1SampleUrl;
            using var response = await httpClientFactory.CreateClient().GetAsync(apiUrl, accessToken, "4321");
            if (response.IsSuccessStatusCode)
            {
                ViewBag.Result = await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception($"Unable to call API. API URL='{apiUrl}', StatusCode='{response.StatusCode}'");
            }

            ViewBag.Title = "Call AspNetCoreApi1Sample";
            return View("CallApi");
        }


        [Authorize]
        public async Task<IActionResult> CallApi1WhichCallApi2()
        {
            var accessToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            var apiUrl = appSettings.AspNetCoreApi1SampleUrl + "api2";
            using var response = await httpClientFactory.CreateClient().GetAsync(apiUrl, accessToken, "4321");
            if (response.IsSuccessStatusCode)
            {
                ViewBag.Result = await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception($"Unable to call API. API URL='{apiUrl}', StatusCode='{response.StatusCode}'");
            }

            ViewBag.Title = "Call API1 which call API2";
            return View("CallApi");
        }

        [Authorize]
        public async Task<IActionResult> TokenExchangeAndApi2()
        {
            var accessToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            var oidcDiscovery = await oidcDiscoveryHandler.GetOidcDiscoveryAsync();

            var tokenExchangeRequest = new TokenExchangeRequest
            {
                Scope = "aspnetcore_api2_sample:some_2_access",
                SubjectToken = accessToken,
                SubjectTokenType = IdentityConstants.TokenTypeIdentifiers.AccessToken
            };

            var tokenExchangeResponse = await tokenExecuteHelper.ExecuteTokenRequestWithClientCredentialGrantAsync<TokenExchangeRequest, TokenExchangeResponse>(identitySettings.ClientId, identitySettings.ClientSecret, tokenEndpoint: oidcDiscovery.TokenEndpoint, tokenRequest: tokenExchangeRequest);

            var apiUrl = appSettings.AspNetCoreApi2SampleUrl;
            using var response = await httpClientFactory.CreateClient().GetAsync(apiUrl, tokenExchangeResponse.AccessToken, "4321");
            if (response.IsSuccessStatusCode)
            {
                ViewBag.Result = await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception($"Unable to call API2. API URL='{apiUrl}', StatusCode='{response.StatusCode}'");
            }

            ViewBag.Title = "Token Exchange + Call Api2";
            return View("CallApi");
        }

        [Authorize]
        public async Task<IActionResult> CallUserInfo()
        {
            var accessToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            var oidcDiscovery = await oidcDiscoveryHandler.GetOidcDiscoveryAsync();

            using var response = await httpClientFactory.CreateClient().GetAsync(oidcDiscovery.UserInfoEndpoint, accessToken);
            if (response.IsSuccessStatusCode)
            {
                ViewBag.Result = $"UserInfo URL='{oidcDiscovery.UserInfoEndpoint}'{Environment.NewLine}{JToken.Parse(await response.Content.ReadAsStringAsync())}";
            }
            else
            {
                ViewBag.Result = $"Unable to call API. API URL='{oidcDiscovery.UserInfoEndpoint}', StatusCode='{response.StatusCode}'";

                var wwwAuthenticateHeader = string.Join(" ", response.Headers.WwwAuthenticate.Select(h => $"{h.Scheme}{(h.Parameter.IsNullOrWhiteSpace() ? string.Empty : $"='{h.Parameter}'")}"));
                ViewBag.Result = $"{ViewBag.Result}{Environment.NewLine}WWWAuthenticate header: {wwwAuthenticateHeader}";
            }

            ViewBag.Title = "Call UserInfo endpoint";
            return View("CallApi");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}