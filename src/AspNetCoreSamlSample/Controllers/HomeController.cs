using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ITfoxtec.Identity.Saml2;
using Microsoft.Extensions.Options;
using ITfoxtec.Identity;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using AspNetCoreSamlSample.Models;
using ITfoxtec.Identity.Helpers;
using ITfoxtec.Identity.Util;
using ITfoxtec.Identity.Messages;
using System.IO;

namespace AspNetCoreSamlSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly Settings settings;
        private readonly Saml2Configuration saml2Config;
        private readonly TokenExecuteHelper tokenExecuteHelper;
        private readonly IHttpClientFactory httpClientFactory;

        public HomeController(IOptionsMonitor<Settings> Settings, IOptionsMonitor<Saml2Configuration> configAccessor, TokenExecuteHelper tokenExecuteHelper, IHttpClientFactory httpClientFactory)
        {
            settings = Settings.CurrentValue;
            saml2Config = configAccessor.CurrentValue;
            this.tokenExecuteHelper = tokenExecuteHelper;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.PublicCertificate = (await saml2Config.SigningCertificate.ToMSJsonWebKeyAsync()).ToJsonIndented();

            return View();
        }

        [Authorize]
        public IActionResult Secure()
        {
            // The NameIdentifier
            var nameIdentifier = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).Single();

            return View();
        }

        [Authorize]
        public async Task<IActionResult> TokenExchangeAndApi1()
        {
            var tokenExchangeRequest = new TokenExchangeRequest
            {
                Scope = "aspnetcore_api1_sample:some_access",
                ActorToken = User.Identities.First().BootstrapContext as string,
                ActorTokenType = IdentityConstants.TokenTypeIdentifiers.Saml2
            };

            var clientCertificate = CertificateUtil.Load(Path.Combine(Startup.AppEnvironment.ContentRootPath, settings.TokenExchangeClientCertificateFile), settings.TokenExchangeClientCertificatePassword);
            var tokenExchangeResponse = await tokenExecuteHelper.ExecuteTokenRequestWithAssertionClientCredentialGrantAsync<TokenExchangeRequest, TokenExchangeResponse>(clientCertificate, settings.TokenExchangeClientId, tokenEndpoint: settings.TokenExchangeTokenEndpoint, tokenRequest: tokenExchangeRequest);

            using var response = await httpClientFactory.CreateClient().GetAsync(settings.AspNetCoreApi1SampleUrl, tokenExchangeResponse.AccessToken, "4321");
            if (response.IsSuccessStatusCode)
            {
                ViewBag.Result = await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception($"Unable to call API. API URL='{settings.AspNetCoreApi1SampleUrl}', StatusCode='{response.StatusCode}'");
            }

            ViewBag.Title = "Call AspNetCoreApi1Sample";
            return View("CallApi");
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
