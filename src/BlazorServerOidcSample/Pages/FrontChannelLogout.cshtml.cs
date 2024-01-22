using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FoxIDs.SampleHelperLibrary.Identity;
using ITfoxtec.Identity;

namespace BlazorServerOidcSample.Pages
{
    public class FrontChannelLogoutModel : PageModel
    {
        private LogoutMemoryCache logoutMemoryCache;
        public FrontChannelLogoutModel(LogoutMemoryCache logoutMemoryCache)
        {
            this.logoutMemoryCache = logoutMemoryCache;
        }

        public IActionResult OnGet([FromQuery(Name = JwtClaimTypes.Issuer)] string issuer, [FromQuery(Name = JwtClaimTypes.SessionId)] string sessionId)
        {
            logoutMemoryCache.List.Add(sessionId);
            return new EmptyResult();
        }
    }
}
