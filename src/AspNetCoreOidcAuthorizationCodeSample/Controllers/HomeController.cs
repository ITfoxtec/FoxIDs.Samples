﻿using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AspNetCoreOidcAuthorizationCodeSample.Models;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;
using System;

namespace AspNetCoreOidcAuthorizationCodeSample.Controllers
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

            var response = await httpClientFactory.CreateClient().GetAsync(appSettings.AspNetCoreApi1SampleUrl, accessToken, "1234");
            if (response.IsSuccessStatusCode)
            {
                ViewBag.Result = await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception($"Unable to call API. Api url='{appSettings.AspNetCoreApi1SampleUrl}', StatusCode='{response.StatusCode}'");
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
