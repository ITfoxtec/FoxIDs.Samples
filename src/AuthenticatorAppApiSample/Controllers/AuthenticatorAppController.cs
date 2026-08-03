using AuthenticatorAppApiSample.Models;
using AuthenticatorAppApiSample.Models.Api;
using ITfoxtec.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticatorAppApiSample.Controllers;

[ApiController]
public class AuthenticatorAppController : ControllerBase
{
    private readonly ILogger<AuthenticatorAppController> logger;
    private readonly AppSettings appSettings;

    public AuthenticatorAppController(ILogger<AuthenticatorAppController> logger, AppSettings appSettings)
    {
        this.logger = logger;
        this.appSettings = appSettings;
    }

    [HttpPost("notification")]
    public IActionResult Notify([FromBody] AuthenticatorAppRequest request)
    {
        (var apiId, var apiSecret) = HttpContext.Request.Headers.GetAuthorizationHeaderBasic();
        if (!VerifyApiIdAndSecret(apiId, apiSecret))
        {
            return Unauthorized(new ErrorResponse
            {
                Error = Constants.Errors.InvalidApiIdOrSecret,
                ErrorMessage = "Invalid API ID or secret."
            });
        }

        if (!Constants.NotificationTypes.Registered.Equals(request.Type, StringComparison.Ordinal) ||
            !Guid.TryParse(request.RegistrationId, out _))
        {
            return BadRequest(new ErrorResponse
            {
                Error = Constants.Errors.InvalidRequest,
                ErrorMessage = "The notification type or registration ID is invalid."
            });
        }

        // Complete the required synchronous backend work here. Return 200 OK only
        // when another FoxIDs deployment can safely handle the registered app.
        logger.LogInformation(
            "Authenticator app registration '{RegistrationId}' received for user '{UserId}' with email '{Email}', phone '{Phone}' and username '{Username}'.",
            request.RegistrationId,
            request.UserId,
            request.Email,
            request.Phone,
            request.Username);

        return Ok();
    }

    private bool VerifyApiIdAndSecret(string apiId, string apiSecret)
    {
        if (!Constants.BasicAuthApiId.Equals(apiId, StringComparison.Ordinal))
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
