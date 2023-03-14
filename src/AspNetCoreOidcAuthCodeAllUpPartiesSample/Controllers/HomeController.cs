using AspNetCoreOidcAuthCodeAllUpPartiesSample.Models;
using ITfoxtec.Identity;
using ITfoxtec.Identity.Discovery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace AspNetCoreOidcAuthCodeAllUpPartiesSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppSettings appSettings;
        private readonly OidcDiscoveryHandler oidcDiscoveryHandler;
        private readonly IHttpClientFactory httpClientFactory;

        public HomeController(AppSettings appSettings, OidcDiscoveryHandler oidcDiscoveryHandler, IHttpClientFactory httpClientFactory)
        {
            this.appSettings = appSettings;
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
        public async Task<IActionResult> CallAspNetCoreApi1Sample()
        {
            var accessToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            using var response = await httpClientFactory.CreateClient().GetAsync(appSettings.AspNetCoreApi1SampleUrl, accessToken, "4321");
            if (response.IsSuccessStatusCode)
            {
                ViewBag.Result = await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception($"Unable to call API. API URL='{appSettings.AspNetCoreApi1SampleUrl}', StatusCode='{response.StatusCode}'");
            }

            ViewBag.Title = "Call AspNetCoreApi1Sample";
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