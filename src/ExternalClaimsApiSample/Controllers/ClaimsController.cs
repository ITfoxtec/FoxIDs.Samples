using ExternalClaimsApiSample.Models;
using ExternalClaimsApiSample.Models.Api;
using ITfoxtec.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ExternalClaimsApiSample.Controllers
{
    [ApiController]
    [Route("ExternalClaims/[controller]")]
    public class ClaimsController : ControllerBase
    {
        private readonly ILogger<ClaimsController> logger;
        private readonly AppSettings appSettings;

        public ClaimsController(ILogger<ClaimsController> logger, AppSettings appSettings)
        {
            this.logger = logger;
            this.appSettings = appSettings;
        }

        [HttpPost]
        public async Task<ActionResult<ClaimsResponse>> Post([FromBody] ClaimsRequest request)
        {
            (var apiId, var apiSecret) = HttpContext.Request.Headers.GetAuthorizationHeaderBasic();
            if (!VerifyApiIdAndSecret(apiId, apiSecret))
            {
                // Return HTTP 401 and an error (required) if the API call is rejected.
                return Unauthorized(new ErrorResponse { Error = ErrorCodes.InvalidApiIdOrSecret, ErrorDescription = "Invalid API ID or secret." });
            }

            if (!(request.Claims?.Count() > 0))
            {
                throw new Exception("Request claims collection is empty.");
            }
                
            var claims = GetClaims(request.Claims)?.ToList();
            return Ok(new ClaimsResponse { Claims = claims });
        }

        private IEnumerable<ClaimValue> GetClaims(IEnumerable<ClaimValue> requestClaims)
        {
            var subValue = requestClaims.Where(c => c.Type == JwtClaimTypes.Email).Select(c => c.Value).FirstOrDefault();
            if(subValue.IsNullOrWhiteSpace())
            {
                subValue = requestClaims.Where(c => c.Type == JwtClaimTypes.Subject).Select(c => c.Value).FirstOrDefault();
            }

            // Load claims from database
            var newSubValue = $"external-{subValue}";

            return
                [new ClaimValue
                {
                    Type = JwtClaimTypes.Subject, Value = $"somewhere/{newSubValue}"
                },
                //new ClaimValue
                //{
                //    Type = JwtClaimTypes.Email, Value = "my@test.org"
                //},
                //new ClaimValue
                //{
                //    Type = "custom_id", Value = "1234"
                //},
                new ClaimValue
                {
                    Type = JwtClaimTypes.Role, Value = "admin_access"
                },
                new ClaimValue
                {
                    Type = JwtClaimTypes.Role, Value = "read_access"
                },
                new ClaimValue
                {
                    Type = JwtClaimTypes.Role, Value = "write_access"
                }];
        }


        private bool VerifyApiIdAndSecret(string apiId, string apiSecret)
        {
            if (!"external_claims".Equals(apiId, StringComparison.Ordinal))
            {
                logger.LogError("Invalid API ID.");
                return false;
            }

            if (!appSettings.ApiSecret.Equals(apiSecret, StringComparison.Ordinal))
            {
                logger.LogError("Invalid API secret.");
                return false;
            }

            return true;
        }
    }
}
