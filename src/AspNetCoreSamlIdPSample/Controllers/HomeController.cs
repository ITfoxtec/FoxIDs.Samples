using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AspNetCoreSamlIdPSample.Models;
using ITfoxtec.Identity.Saml2;
using Microsoft.Extensions.Options;
using ITfoxtec.Identity;
using FoxIDs.SampleHelperLibrary.Repository;
using System.Threading.Tasks;

namespace AspNetCoreSamlIdPSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly Saml2ConfigurationIdP saml2Config;
        private readonly IdPSessionCookieRepository idPSessionCookieRepository;

        public HomeController(IOptionsMonitor<Saml2ConfigurationIdP> configAccessor, IdPSessionCookieRepository idPSessionCookieRepository)
        {
            saml2Config = configAccessor.CurrentValue;
            this.idPSessionCookieRepository = idPSessionCookieRepository;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.PublicCertificate = (await saml2Config.SigningCertificate.ToMSJsonWebKeyAsync()).ToJsonIndented();
            ViewBag.Session = await idPSessionCookieRepository.GetAsync();

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
