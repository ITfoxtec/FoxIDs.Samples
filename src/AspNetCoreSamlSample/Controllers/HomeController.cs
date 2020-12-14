using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ITfoxtec.Identity.Saml2;
using Microsoft.Extensions.Options;
using ITfoxtec.Identity;

namespace AspNetCoreSamlSample.Controllers
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
            ViewBag.PublicCertificate = saml2Config.SigningCertificate.ToMSJsonWebKeyAsync().ToJsonIndented();

            return View();
        }

        [Authorize]
        public IActionResult Secure()
        {
            // The NameIdentifier
            var nameIdentifier = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).Single();

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
