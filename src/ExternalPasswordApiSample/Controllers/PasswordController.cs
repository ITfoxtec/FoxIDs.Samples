using System.Net;
using System.Text;
using ExternalPasswordApiSample.Models;
using ExternalPasswordApiSample.Models.Api;
using Microsoft.AspNetCore.Mvc;

namespace ExternalPasswordApiSample.Controllers;

[ApiController]
public class PasswordController : ControllerBase
{
    private readonly ILogger<PasswordController> logger;
    private readonly AppSettings appSettings;

    public PasswordController(ILogger<PasswordController> logger, AppSettings appSettings)
    {
        this.logger = logger;
        this.appSettings = appSettings;
    }

    [HttpPost("validation")]
    public IActionResult ValidatePassword([FromBody] PasswordRequest request)
    {
        if (!Authenticate(out var authError))
        {
            return Unauthorized(new ErrorResponse { Error = Constants.Errors.InvalidApiIdSecret, ErrorMessage = authError });
        }

        //if (!ModelState.IsValid)
        //{
        //    return BadRequest(new ErrorResponse { Error = Constants.Errors.PasswordNotAccepted, ErrorMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });
        //}

        // Demo password policy (replace with real logic)
        if (request.Password!.Length < 8 || request.Password.Contains(/*"Formel1bil"*/ "Forbidden!", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode((int)HttpStatusCode.BadRequest, new ErrorResponse
            {
                Error = Constants.Errors.PasswordNotAccepted,
                ErrorMessage = "Password not accepted by external policy.",
                UiErrorMessage = "Password does not meet the required policy which is xxx."
            });
        }

        // Example of also doing notification work here if desired.
        return Ok(); // No body required.
    }

    [HttpPost("notification")]
    public IActionResult NotifyPassword([FromBody] PasswordRequest request)
    {
        if (!Authenticate(out var authError))
        {
            return Unauthorized(new ErrorResponse { Error = Constants.Errors.InvalidApiIdSecret, ErrorMessage = authError });
        }

        //if (!ModelState.IsValid)
        //{
        //    // For notification endpoint we treat validation issues as generic server error (could also be 400)
        //    return BadRequest(new ErrorResponse { Error = "invalid_request", ErrorMessage = "Invalid identifiers or password missing." });
        //}

        // Perform notification logic (e.g., enqueue event, call backend, log, etc.)
        return Ok();
    }

    private bool Authenticate(out string error)
    {
        error = null;
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            error = "Missing Authorization header.";
            return false;
        }

        var value = authHeader.ToString();
        if (!value.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            error = "Authorization header must be Basic.";
            return false;
        }

        try
        {
            var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(value[6..]));
            var sep = credentials.IndexOf(':');
            if (sep <= 0)
            {
                error = "Malformed basic credentials.";
                return false;
            }
            var apiId = credentials[..sep];
            var apiSecret = credentials[(sep + 1)..];

            if (!string.Equals(apiId, Constants.BasicAuthAppId, StringComparison.Ordinal) ||
                !string.Equals(apiSecret, appSettings.ApiSecret, StringComparison.Ordinal))
            {
                if (!Constants.BasicAuthAppId.Equals(apiId, StringComparison.Ordinal))
                {
                    logger.LogError("Invalid API ID.");
                }
                if (!appSettings.ApiSecret.Equals(apiSecret, StringComparison.Ordinal))
                {
                    logger.LogError("Invalid API secret.");
                }

                error = "Invalid API ID or secret.";
                return false;
            }
            return true;
        }
        catch
        {
            error = "Invalid base64 in Authorization header.";
            return false;
        }
    }
}
