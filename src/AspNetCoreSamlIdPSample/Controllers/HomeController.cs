using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AspNetCoreSamlIdPSample.Models;
using ITfoxtec.Identity.Saml2;
using Microsoft.Extensions.Options;
using ITfoxtec.Identity;

namespace AspNetCoreSamlIdPSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly Saml2Configuration saml2Config;

        public HomeController(IOptionsMonitor<Saml2Configuration> configAccessor)
        {
            saml2Config = configAccessor.CurrentValue;
        }

        public IActionResult Index()
        {
            ViewBag.PublicCertificate = saml2Config.SigningCertificate.ToJsonWebKeyAsync().ToJsonIndented();

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
