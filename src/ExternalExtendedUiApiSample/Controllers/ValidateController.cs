using ExternalExtendedUiApiSample.Models;
using ExternalExtendedUiApiSample.Models.Api;
using ITfoxtec.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace ExternalExtendedUiApiSample.Controllers
{
    [ApiController]
    [Route("ExtendedUi/[controller]")]
    public class ValidateController : ControllerBase
    {
        private readonly ILogger<ValidateController> logger;
        private readonly AppSettings appSettings;

        public ValidateController(ILogger<ValidateController> logger, AppSettings appSettings)
        {
            this.logger = logger;
            this.appSettings = appSettings;
        }

        [HttpPost]
        public async Task<ActionResult<ExtendedUiResponse>> Post([FromBody] ExtendedUiRequest request)
        {
            (var apiId, var apiSecret) = HttpContext.Request.Headers.GetAuthorizationHeaderBasic();
            if (!VerifyApiIdAndSecret(apiId, apiSecret))
            {
                // Return HTTP 401 and an error (required) if the API call is rejected.
                return Unauthorized(new ErrorResponse { Error = ErrorCodes.InvalidApiIdOrSecret, ErrorMessage = "Invalid API ID or secret." });
            }

            var claims = new List<ClaimValue>();

            if (request.Claims?.Count() > 0)
            {
                // maybe do something...
            }

            claims.Add(new ClaimValue { Type = "my_custom_info_claim", Value = "some information" });

            if (request.Elements?.Count() > 0)
            {
                // validate and use the element input

                foreach (var element in request.Elements)
                {
                    if (element.Name == "ktvywqwc")
                    {
                        if (element.Value == "111")
                        {
                            // Display generic error message
                            return BadRequest(new ErrorResponse { Error = ErrorCodes.Invalid, ErrorMessage = $"Invalid value '{element.Value}' in element '{element.Name}'." });
                        }

                        if (element.Value == "222")
                        {
                            // Display generic error message
                            return BadRequest(new ErrorResponse
                            {
                                Error = ErrorCodes.Invalid,
                                Elements = [new ElementError { Name = element.Name }]
                            });
                        }

                        if (element.Value == "333")
                        {
                            // Display field error message
                            return BadRequest(new ErrorResponse
                            {
                                Error = ErrorCodes.Invalid,
                                Elements = [new ElementError { Name = element.Name, UiErrorMessage = $"Please use another value." }]
                            });
                        }

                        if (element.Value == "444")
                        {
                            // Display unknown field error message with generic error message
                            return BadRequest(new ErrorResponse
                            {
                                Error = ErrorCodes.Invalid,
                                Elements = [new ElementError { Name = "incorrect_name", UiErrorMessage = $"Please use another value." }]
                            });
                        }

                        if (element.Value == "123456")
                        {
                            // Accept value and add it as a claim
                            claims.Add(new ClaimValue { Type = element.ClaimType ?? "my_custom_field_claim", Value = $"{element.Type}-Element-{element.Value}"  });
                        }
                    }
                }
            }

            return Ok(new ExtendedUiResponse { Claims = claims });
        }

        private bool VerifyApiIdAndSecret(string apiId, string apiSecret)
        {
            if (!"external_extended_ui".Equals(apiId, StringComparison.Ordinal))
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
