using ExternalLoginApiSample.Models;
using ExternalLoginApiSample.Models.Api;
using ITfoxtec.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ExternalLoginApiSample.Controllers
{
    [ApiController]
    [Route("ExternalLogin/[controller]")]
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
        public async Task<ActionResult<AuthenticationResponse>> Post([FromBody] AuthenticationRequest request)
        {
            (var apiId, var apiSecret) = HttpContext.Request.Headers.GetAuthorizationHeaderBasic();
            if (!VerifyApiIdAndSecret(apiId, apiSecret))
            {
                // Return HTTP 401 and an error (required) if the API call is rejected.
                return Unauthorized(new ErrorResponse { Error = ErrorCodes.InvalidAPIIDOrSecret, ErrorDescription = "Invalid API ID or secret." });
            }

            // Include if only one username type is supported.
            //if (request.UsernameType != ExternalLoginUsernameTypes.Text)
            //{
            //    // Return HTTP 400 if an error occurs.
            //    return BadRequest("Only text based usernames is supported.");
            //}

            // Test users with an email as username.
            //var claims = ValidateByEmailbasedUsername(request.Username, request.Password);
            // Test users with a text-based username.
            var claims = ValidateByTextbasedUsername(request.Username, request.Password)?.ToList();

            if (claims?.Count() > 0)
            {
                // Your own custom ID added as an additional parameter.
                if (!request.SomeCustomId.IsNullOrWhiteSpace())
                {
                    claims.Add(new ClaimValue { Type = "SomeCustomId", Value = request.SomeCustomId });
                }

                return Ok(new AuthenticationResponse { Claims = claims });
            }

            // Return HTTP 400 or 401 or 403 and an error (required) if the username or password combination is invalid.
            return Unauthorized(new ErrorResponse { Error = ErrorCodes.InvalidUsernameOrPassword, ErrorDescription = "Invalid username or password." });
        }

        private bool VerifyApiIdAndSecret(string apiId, string apiSecret)
        {
            if (!"external_login".Equals(apiId, StringComparison.Ordinal))
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

        private IEnumerable<ClaimValue> ValidateByEmailbasedUsername(string email, string password)
        {
            email = email.ToLower();
            if (email.Equals("user1@somewhere.org", StringComparison.Ordinal) && password.Equals("testpass1", StringComparison.Ordinal))
            {
                return
                    [new ClaimValue
                    {
                        Type = JwtClaimTypes.Subject, Value = $"somewhere/{email}"
                    }];
            }
            if (email.Equals("user2@somewhere.org", StringComparison.Ordinal) && password.Equals("testpass2", StringComparison.Ordinal))
            {
                return
                    [new ClaimValue
                    {
                        Type = JwtClaimTypes.Subject, Value = $"somewhere/{email}"
                    },
                    new ClaimValue
                    {
                        Type = JwtClaimTypes.Email, Value = email
                    },
                    new ClaimValue
                    {
                        Type = JwtClaimTypes.Role, Value = "admin_access"
                    }];
            }

            return null;
        }

        private IEnumerable<ClaimValue> ValidateByTextbasedUsername(string username, string password)
        {
            username = username.ToLower();
            if (username.Equals("user1", StringComparison.Ordinal) && password.Equals("testpass1", StringComparison.Ordinal))
            {
                return
                    [new ClaimValue
                    {
                        Type = JwtClaimTypes.Subject, Value = $"somewhere/{username}"
                    }];
            }
            if (username.Equals("user2", StringComparison.Ordinal) && password.Equals("testpass2", StringComparison.Ordinal))
            {
                return
                    [new ClaimValue
                    {
                        Type = JwtClaimTypes.Subject, Value = $"somewhere/{username}"
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
