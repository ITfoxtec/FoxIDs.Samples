using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using ITfoxtec.Identity;
using FoxIDs.SampleHelperLibrary.Identity;

namespace AspNetCoreOidcAuthCodeAllUpPartiesSample.Controllers
{
    public class AuthController : Controller
    {
        private LogoutMemoryCache logoutMemoryCache;
        public AuthController(LogoutMemoryCache logoutMemoryCache)
        {
            this.logoutMemoryCache = logoutMemoryCache;
        }

        public IActionResult Login()
        {
            var redirectUrl = Url.Action(nameof(HomeController.Secure), "Home");
            return Challenge(new AuthenticationProperties { RedirectUri = redirectUrl }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            var callbackUrl = Url.Action(nameof(HomeController.Index), "Home", values: null, protocol: Request.Scheme);
            return SignOut(new AuthenticationProperties { RedirectUri = callbackUrl },
                CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }

        public IActionResult FrontChannelLogout([FromQuery(Name = JwtClaimTypes.Issuer)] string issuer, [FromQuery(Name = JwtClaimTypes.SessionId)] string sessionId)
        {
            logoutMemoryCache.List.Add(sessionId);
            return Ok();
        }
    }
}
