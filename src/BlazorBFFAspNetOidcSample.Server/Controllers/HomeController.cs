using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BlazorBFFAspNetOidcSample.Server.Models;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http;
using System;
using Microsoft.AspNetCore.Http;
using UrlCombineLib;

namespace BlazorBFFAspNetOidcSample.Server.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppSettings appSettings;
        private readonly IHttpClientFactory httpClientFactory;

        public HomeController(AppSettings appSettings, IHttpClientFactory httpClientFactory)
        {
            this.appSettings = appSettings;
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

            var appUrl = UrlCombine.Combine(appSettings.AspNetCoreApi1SampleUrl, "values");
            using var response = await httpClientFactory.CreateClient().GetAsync(appUrl, accessToken, "1234");
            if (response.IsSuccessStatusCode)
            {
                ViewBag.Result = await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception($"Unable to call API. Api url='{appUrl}', StatusCode='{response.StatusCode}'");
            }

            ViewBag.Title = "Call AspNetCoreApi1Sample";
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
