using DirectoryConnectorApiSample.Models;
using DirectoryConnectorApiSample.Models.Api;
using DirectoryConnectorApiSample.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;

namespace DirectoryConnectorApiSample.Controllers;

[ApiController]
[Route("DirectoryConnector")]
public class DirectoryConnectorController : ControllerBase
{
    private readonly ILogger<DirectoryConnectorController> logger;
    private readonly AppSettings appSettings;
    private readonly DemoDirectoryStore directoryStore;

    public DirectoryConnectorController(ILogger<DirectoryConnectorController> logger, AppSettings appSettings, DemoDirectoryStore directoryStore)
    {
        this.logger = logger;
        this.appSettings = appSettings;
        this.directoryStore = directoryStore;
    }

    [HttpPost("authentication")]
    public IActionResult Authenticate([FromBody] DirectoryAuthenticationRequest request)
    {
        if (!AuthenticateApi(out var authError))
        {
            return Unauthorized(new ErrorResponse { Error = Constants.Errors.InvalidApiIdOrSecret, ErrorMessage = authError });
        }

        var user = directoryStore.Find(request);
        var userError = ValidateUser(user);
        if (userError != null)
        {
            return userError;
        }

        if (!directoryStore.ValidatePassword(user, request.Password))
        {
            return Unauthorized(new ErrorResponse { Error = Constants.Errors.InvalidUsernameOrPassword, ErrorMessage = "Invalid username or password." });
        }

        return Ok(user.ToResponse());
    }

    [HttpPost("change-password")]
    public IActionResult ChangePassword([FromBody] DirectoryChangePasswordRequest request)
    {
        if (!AuthenticateApi(out var authError))
        {
            return Unauthorized(new ErrorResponse { Error = Constants.Errors.InvalidApiIdOrSecret, ErrorMessage = authError });
        }

        var user = directoryStore.Find(request);
        var userError = ValidateUser(user);
        if (userError != null)
        {
            return userError;
        }

        if (!directoryStore.ValidatePassword(user, request.CurrentPassword))
        {
            return Unauthorized(new ErrorResponse { Error = Constants.Errors.InvalidCurrentPassword, ErrorMessage = "Invalid current password." });
        }

        var passwordError = ValidateNewPassword(request.NewPassword, user, request.CurrentPassword);
        if (passwordError != null)
        {
            return passwordError;
        }

        directoryStore.SetPassword(user, request.NewPassword);
        return Ok(user.ToResponse());
    }

    [HttpPost("set-password")]
    public IActionResult SetPassword([FromBody] DirectorySetPasswordRequest request)
    {
        if (!AuthenticateApi(out var authError))
        {
            return Unauthorized(new ErrorResponse { Error = Constants.Errors.InvalidApiIdOrSecret, ErrorMessage = authError });
        }

        var user = directoryStore.Find(request);
        var userError = ValidateUser(user);
        if (userError != null)
        {
            return userError;
        }

        var passwordError = ValidateNewPassword(request.Password, user);
        if (passwordError != null)
        {
            return passwordError;
        }

        directoryStore.SetPassword(user, request.Password);
        return Ok(user.ToResponse());
    }

    private IActionResult ValidateUser(DemoDirectoryUser user)
    {
        if (user == null)
        {
            return Unauthorized(new ErrorResponse { Error = Constants.Errors.InvalidUsernameOrPassword, ErrorMessage = "User not found." });
        }
        if (user.Deleted)
        {
            return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse { Error = Constants.Errors.UserDeleted, ErrorMessage = "User is deleted in the directory." });
        }
        if (user.Disabled)
        {
            return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse { Error = Constants.Errors.UserDisabled, ErrorMessage = "User is disabled in the directory." });
        }

        return null;
    }

    private IActionResult ValidateNewPassword(string password, DemoDirectoryUser user, string currentPassword = null)
    {
        if (password?.Length < 8)
        {
            return BadRequest(new ErrorResponse { Error = Constants.Errors.PasswordMinLength, ErrorMessage = "Password must be at least 8 characters." });
        }
        if (password.Contains("Forbidden!", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new ErrorResponse { Error = Constants.Errors.PasswordBannedCharacters, ErrorMessage = "Password contains a banned word." });
        }
        if (!string.IsNullOrWhiteSpace(currentPassword) && password.Equals(currentPassword, StringComparison.Ordinal))
        {
            return BadRequest(new ErrorResponse { Error = Constants.Errors.NewPasswordEqualsCurrent, ErrorMessage = "New password equals current password." });
        }
        if (password.Contains(user.Username, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new ErrorResponse { Error = Constants.Errors.PasswordBannedCharacters, ErrorMessage = "Demo policy rejects passwords containing the username." });
        }

        return null;
    }

    private bool AuthenticateApi(out string error)
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
                error = "Malformed Basic credentials.";
                return false;
            }

            var apiId = credentials[..sep];
            var apiSecret = credentials[(sep + 1)..];
            if (!Constants.BasicAuthAppId.Equals(apiId, StringComparison.Ordinal))
            {
                logger.LogError("Invalid API ID.");
                error = "Invalid API ID or secret.";
                return false;
            }
            if (!appSettings.ApiSecret.Equals(apiSecret, StringComparison.Ordinal))
            {
                logger.LogError("Invalid API secret.");
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
