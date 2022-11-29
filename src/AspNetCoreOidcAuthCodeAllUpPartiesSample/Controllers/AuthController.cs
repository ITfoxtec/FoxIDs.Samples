using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using ITfoxtec.Identity;

namespace AspNetCoreOidcAuthCodeAllUpPartiesSample.Controllers
{
    public class AuthController : Controller
    {
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

        public async Task<IActionResult> FrontChannelLogout([FromQuery(Name = JwtClaimTypes.Issuer)] string issuer, [FromQuery(Name = JwtClaimTypes.SessionId)] string sessionId)
        {
            if(User.Identity.IsAuthenticated)
            {
                if (User.Claims.Where(c => c.Type == JwtClaimTypes.SessionId && c.Issuer == issuer && c.Value == sessionId).Any())
                {
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }
                else
                {
                    return BadRequest("Invalid issuer and session ID.");
                }
            }

            return Ok();
        }
    }
}
