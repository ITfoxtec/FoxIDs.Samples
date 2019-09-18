using AspNetCoreOidcImplicitSample.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using FoxIDs.SampleHelperLibrary.Models;

namespace AspNetCoreOidcImplicitSample.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Login(LoginType? loginType = null)
        {
            var redirectUrl = Url.Action(nameof(HomeController.Secure), "Home");
            var authenticationProperties = new AuthenticationProperties { RedirectUri = redirectUrl };
            if(loginType.HasValue)
            {
                authenticationProperties.Items.Add(Constants.StateLoginType, loginType.Value.ToString());
            }
            return Challenge(authenticationProperties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        public IActionResult Logout()
        {
            var callbackUrl = Url.Action(nameof(HomeController.Index), "Home", values: null, protocol: Request.Scheme);
            return SignOut(new AuthenticationProperties { RedirectUri = callbackUrl },
                CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}
