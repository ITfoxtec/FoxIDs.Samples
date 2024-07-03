using ExternalLoginApiSample.Models;
using ExternalLoginApiSample.Models.Api;
using ITfoxtec.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ExternalLoginApiSample.Controllers
{
    [ApiController]
    [Route("ExternalLoginApi/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly ILogger<AuthenticationController> logger;
        private readonly AppSettings appSettings;

        public AuthenticationController(ILogger<AuthenticationController> logger, AppSettings appSettings)
        {
            this.logger = logger;
            this.appSettings = appSettings;
        }

        [HttpPost]
        public async Task<ActionResult<AuthenticationResponse>> Post([FromHeader(Name = "api_secret")] string apiSecret, [FromBody] AuthenticationRequest request)
        {
            if (!VerifyApiSecret(apiSecret))
            {
                return Problem("Invalid API secret.");
            }

            var claims = ValidateUser(request.Username, request.Password);
            if (claims?.Count() > 0)
            {
                return Ok(new AuthenticationResponse { Claims = claims });
            }

            return Unauthorized("Invalid username or password.");
        }

        private bool VerifyApiSecret(string apiSecret)
        {
            if (apiSecret.Equals(appSettings.ApiSecret, StringComparison.Ordinal))
            {
                return true;
            }

            logger.LogError("Invalid API secret.");
            return false;
        }

        private IEnumerable<ClaimValue> ValidateUser(string username, string password)
        {
            username = username.ToLower();
            if (username.Equals("user1", StringComparison.Ordinal) && password.Equals("testpass1", StringComparison.Ordinal))
            {
                return
                    [new ClaimValue
                    {
                        Type = JwtClaimTypes.Subject, Value = $"custom-api/{username}"
                    }];
            }
            if (username.Equals("user2", StringComparison.Ordinal) && password.Equals("testpass2", StringComparison.Ordinal))
            {
                return
                    [new ClaimValue
                    {
                        Type = JwtClaimTypes.Subject, Value = $"custom-api/{username}"
                    },
                    new ClaimValue
                    {
                        Type = JwtClaimTypes.Email, Value = $"{username}@somewhere.org"
                    },
                    new ClaimValue
                    {
                        Type = JwtClaimTypes.Role, Value = "admin_access"
                    }];
            }

            return null;
        }
    }
}
