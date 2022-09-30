using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using ITfoxtec.Identity;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;

namespace BlazorBFFAspNetOidcSample.Server.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAntiforgery antiforgery;

        public AuthController(IAntiforgery antiforgery)
        {
            this.antiforgery = antiforgery;
        }

        public IActionResult Login(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            var authenticationProperties = new AuthenticationProperties { RedirectUri = returnUrl };
            return Challenge(authenticationProperties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            var returnUrl = Url.Action(nameof(HomeController.Index), "Home", values: null, protocol: Request.Scheme);
            return Logout(returnUrl);
        }

        public async Task<IActionResult> BlazorLogout(string antiforgeryToken)
        {
            HttpContext.Request.Headers[Constants.AntiforgeryTokenHeaderName] = antiforgeryToken;
            await antiforgery.ValidateRequestAsync(HttpContext);

            var returnUrl = Url.Content("~/");
            return Logout(returnUrl);
        }

        private IActionResult Logout(string returnUrl)
        {
            return SignOut(new AuthenticationProperties { RedirectUri = returnUrl },
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
