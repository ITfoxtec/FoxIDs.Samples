using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using FoxIDs.SampleHelperLibrary.Models;
using ITfoxtec.Identity;

namespace BlazorServerOidcSample.Pages
{
    public class LoginModel : PageModel
    {
        public IActionResult OnGet(string redirectUri, LoginType? loginType = null)
        {
            if (redirectUri.IsNullOrWhiteSpace())
            {
                redirectUri = Url.Content("~/");
            }

            var authenticationProperties = new AuthenticationProperties { RedirectUri = redirectUri };
            if (loginType.HasValue)
            {
                authenticationProperties.Items.Add(Constants.StateLoginType, loginType.Value.ToString());
            }
            return Challenge(authenticationProperties, OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}
