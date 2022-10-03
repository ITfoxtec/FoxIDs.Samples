using Microsoft.AspNetCore.Mvc;
using BlazorBFFAspNetOidcSample.Models.Api;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;

namespace BlazorBFFAspNetOidcSample.Server.Controllers.Api
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AntiforgeryTokenController : ControllerBase
    {
        private readonly IAntiforgery antiforgery;

        public AntiforgeryTokenController(IAntiforgery antiforgery)
        {
            this.antiforgery = antiforgery;
        }

        /// <summary>
        /// Return the first Antiforgery token.
        /// </summary>
        [HttpGet]
        public ActionResult<AntiforgeryTokenResponse> Get()
        {
            var tokens = antiforgery.GetAndStoreTokens(HttpContext);
            return new AntiforgeryTokenResponse
            {
                Token = tokens.RequestToken!,
            }; 
        }
    }
}
