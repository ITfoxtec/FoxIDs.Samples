using Microsoft.AspNetCore.Mvc;
using System.Linq;
using ITfoxtec.Identity;
using BlazorBFFAspNetOidcSample.Models.Api;
using Microsoft.AspNetCore.Authorization;

namespace BlazorBFFAspNetOidcSample.Server.Controllers.Api
{
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        /// <summary>
        /// Return the users identity.
        /// </summary>
        [HttpGet]
        public ActionResult<IdentityResponse> Get()
        {
            var identityResponse = new IdentityResponse
            {
                IsAuthenticated = User.Identity.IsAuthenticated,
            };

            if (identityResponse.IsAuthenticated)
            {
                identityResponse.Username = User.Claims.Where(c => c.Type == JwtClaimTypes.Subject).Select(c => c.Value).FirstOrDefault();
                identityResponse.GivenName = User.Claims.Where(c => c.Type == JwtClaimTypes.GivenName).Select(c => c.Value).FirstOrDefault();
                identityResponse.FamilyName = User.Claims.Where(c => c.Type == JwtClaimTypes.FamilyName).Select(c => c.Value).FirstOrDefault();
                identityResponse.Claims = User.Claims.Select(c => new ClaimValue { Type = c.Type, Value = c.Value });
            }

            return identityResponse;
        }
    }
}
