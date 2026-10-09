using DirectoryConnectorApiSample.Models;
using DirectoryConnectorApiSample.Models.Api;
using DirectoryConnectorApiSample.Services;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;
using System.Text;

namespace DirectoryConnectorApiSample.Controllers;

[ApiController]
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

    [HttpGet("health")]
    public IActionResult Health()
    {
        // TODO: Validate required configuration, for example the API secret and directory connection settings.
        // TODO: Check that the external directory can be reached and queried.
        // TODO: Check that the connector identity has the required read/write access for users and password changes.
        return Ok(new
        {
            status = "ok",
            configuration = "ok",
            directory = "ok",
            accessRights = "ok"
        });
    }

    [HttpPost("synchronise-user")]
    [ProducesResponseType(typeof(DirectoryUserResponse), StatusCodes.Status200OK)]
    public IActionResult SynchroniseUser([FromBody] DirectorySynchronisationRequest request)
    {
        if (!AuthenticateApi(out var authError))
        {
            return Unauthorized(new ErrorResponse { Error = Constants.Errors.InvalidApiIdOrSecret, ErrorMessage = authError });
        }

        var user = directoryStore.Find(request);
        if (user == null || user.Deleted)
        {
            return string.IsNullOrWhiteSpace(request.DirectoryUserId)
                ? Unauthorized(new ErrorResponse { Error = Constants.Errors.UserNotExists, ErrorMessage = "No directory user matches the supplied identifier." })
                : StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse { Error = Constants.Errors.UserDeleted, ErrorMessage = "The directory user ID no longer exists." });
        }

        return Ok(user.ToResponse());
    }

    [HttpPost("authentication")]
    public IActionResult Authenticate([FromBody] DirectoryAuthenticationRequest request)
    {
        if (!AuthenticateApi(out var authError))
        {
            return Unauthorized(new ErrorResponse { Error = Constants.Errors.InvalidApiIdOrSecret, ErrorMessage = authError });
        }

        var user = directoryStore.Find(request.DirectoryUserId);
        var userError = ValidatePasswordOperationUser(user);
        if (userError != null)
        {
            return userError;
        }

        if (!directoryStore.ValidatePassword(user, request.Password))
        {
            return Unauthorized(new ErrorResponse { Error = Constants.Errors.InvalidPassword, ErrorMessage = "Invalid password." });
        }

        // Only return account-specific login guidance after the credentials have been verified.
        if (user.RejectLogin)
        {
            const string errorMessage = "Credentials verified; login rejected by demo directory policy.";
            logger.LogInformation("Login rejected for directory user {DirectoryUserId}. {Reason}", user.DirectoryUserId, errorMessage);
            return Unauthorized(new ErrorResponse
            {
                Error = Constants.Errors.LoginRejected,
                ErrorMessage = errorMessage,
                UiErrorMessage = user.ShowLoginRejectionMessage
                    ? CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
                    {
                        "da" => "Du kan ikke logge ind her. Kontakt support.",
                        _ => "You cannot log in here. Contact support."
                    }
                    : null
            });
        }

        if (user.PasswordExpired)
        {
            return BadRequest(new ErrorResponse { Error = Constants.Errors.PasswordExpired, ErrorMessage = "The verified password has expired." });
        }

        var passwordError = ValidatePassword(request.Password, user.Username);
        if (passwordError != null)
        {
            return passwordError;
        }

        return NoContent();
    }

    [HttpPost("create-user")]
    [ProducesResponseType(typeof(DirectoryCreateUserResponse), StatusCodes.Status200OK)]
    public IActionResult CreateUser([FromBody] DirectoryCreateUserRequest request)
    {
        if (!AuthenticateApi(out var authError))
        {
            return Unauthorized(new ErrorResponse { Error = Constants.Errors.InvalidApiIdOrSecret, ErrorMessage = authError });
        }

        var passwordError = ValidateNewPassword(request.Password, request);
        if (passwordError != null)
        {
            return passwordError;
        }

        var user = directoryStore.Create(request);
        if (user == null)
        {
            return BadRequest(new ErrorResponse { Error = Constants.Errors.UserExists, ErrorMessage = "User already exists." });
        }
        return Ok(new DirectoryCreateUserResponse { DirectoryUserId = user.DirectoryUserId });
    }

    [HttpPost("change-password")]
    public IActionResult ChangePassword([FromBody] DirectoryChangePasswordRequest request)
    {
        if (!AuthenticateApi(out var authError))
        {
            return Unauthorized(new ErrorResponse { Error = Constants.Errors.InvalidApiIdOrSecret, ErrorMessage = authError });
        }

        var user = directoryStore.Find(request.DirectoryUserId);
        var userError = ValidatePasswordOperationUser(user);
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
        return NoContent();
    }

    [HttpPost("set-password")]
    public IActionResult SetPassword([FromBody] DirectorySetPasswordRequest request)
    {
        if (!AuthenticateApi(out var authError))
        {
            return Unauthorized(new ErrorResponse { Error = Constants.Errors.InvalidApiIdOrSecret, ErrorMessage = authError });
        }

        var user = directoryStore.Find(request.DirectoryUserId);
        var userError = ValidatePasswordOperationUser(user);
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
        return NoContent();
    }

    private IActionResult ValidatePasswordOperationUser(DemoDirectoryUser user)
    {
        if (user == null || user.Deleted || user.Disabled)
        {
            return StatusCode((int)HttpStatusCode.Forbidden, new ErrorResponse { Error = Constants.Errors.OperationRejected, ErrorMessage = "The directory account is unavailable." });
        }

        return null;
    }

    private IActionResult ValidatePassword(string password, string username)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            return BadRequest(new ErrorResponse { Error = Constants.Errors.PasswordMinLength, ErrorMessage = "Password must be at least 8 characters." });
        }
        if (password.Contains("Forbidden!", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new ErrorResponse { Error = Constants.Errors.PasswordBannedCharacters, ErrorMessage = "Password contains a banned word." });
        }
        if (!string.IsNullOrWhiteSpace(username) && password.Contains(username, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new ErrorResponse { Error = Constants.Errors.PasswordBannedCharacters, ErrorMessage = "Demo policy rejects passwords containing the username." });
        }

        return null;
    }

    private IActionResult ValidateNewPassword(string password, DemoDirectoryUser user, string currentPassword = null)
    {
        return ValidateNewPassword(password, user.Username, currentPassword);
    }

    private IActionResult ValidateNewPassword(string password, DirectoryCreateUserRequest request)
    {
        return ValidateNewPassword(password, request.Username, currentPassword: null);
    }

    private IActionResult ValidateNewPassword(string password, string username, string currentPassword = null)
    {
        var passwordError = ValidatePassword(password, username);
        if (passwordError != null)
        {
            return passwordError;
        }

        if (!string.IsNullOrWhiteSpace(currentPassword) && password.Equals(currentPassword, StringComparison.Ordinal))
        {
            return BadRequest(new ErrorResponse { Error = Constants.Errors.NewPasswordEqualsCurrent, ErrorMessage = "New password equals current password." });
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
