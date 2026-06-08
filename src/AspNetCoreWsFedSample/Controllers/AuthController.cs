using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreWsFedSample.Controllers;

public class AuthController : Controller
{
    public IActionResult Login(string? returnUrl = null)
    {
        var redirectUrl = !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? returnUrl
            : Url.Action(nameof(HomeController.Secure), "Home");
        return Challenge(new AuthenticationProperties { RedirectUri = redirectUrl }, WsFederationDefaults.AuthenticationScheme);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        var callbackUrl = Url.Action(nameof(HomeController.Index), "Home", values: null, protocol: Request.Scheme);
        return SignOut(new AuthenticationProperties { RedirectUri = callbackUrl },
            CookieAuthenticationDefaults.AuthenticationScheme,
            WsFederationDefaults.AuthenticationScheme);
    }
}
