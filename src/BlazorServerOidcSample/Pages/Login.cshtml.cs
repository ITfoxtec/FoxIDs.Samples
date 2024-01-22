using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using ITfoxtec.Identity;

namespace BlazorServerOidcSample.Pages
{
    public class LoginModel : PageModel
    {
        public IActionResult OnGet(string redirectUri)
        {
            if (redirectUri.IsNullOrWhiteSpace())
            {
                redirectUri = Url.Content("~/");
            }
            return Challenge(new AuthenticationProperties { RedirectUri = redirectUri }, OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}
