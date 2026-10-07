using DirectoryConnectorApiSample.Models.Api;

namespace DirectoryConnectorApiSample.Services;

public class DemoDirectoryUser
{
    public string DirectoryUserId { get; set; }

    public string Email { get; set; }

    public string Phone { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public bool RejectLogin { get; set; }

    public bool ShowLoginRejectionMessage { get; set; }

    public bool Disabled { get; set; }

    public bool Deleted { get; set; }

    public bool ConfirmAccount { get; set; }

    public bool ChangePassword { get; set; }

    public bool PasswordExpired { get; set; }

    public bool SetPasswordEmail { get; set; }

    public bool SetPasswordSms { get; set; }

    public bool DisableSetPasswordEmail { get; set; }

    public bool DisableSetPasswordSms { get; set; }

    public long? PasswordLastChanged { get; set; }

    public bool EmailVerified { get; set; }

    public bool PhoneVerified { get; set; }

    public bool DisableTwoFactorApp { get; set; }

    public bool DisableTwoFactorSms { get; set; }

    public bool DisableTwoFactorEmail { get; set; }

    public bool RequireMultiFactor { get; set; }

    public List<ClaimValue> Claims { get; set; } = [];

    public DirectoryUserResponse ToResponse() => new()
    {
        DirectoryUserId = DirectoryUserId,
        Email = Email,
        Phone = Phone,
        Username = Username,
        ConfirmAccount = ConfirmAccount,
        DisableAccount = Disabled,
        ChangePassword = ChangePassword || PasswordExpired,
        SetPasswordEmail = SetPasswordEmail,
        SetPasswordSms = SetPasswordSms,
        DisableSetPasswordEmail = DisableSetPasswordEmail,
        DisableSetPasswordSms = DisableSetPasswordSms,
        PasswordLastChanged = PasswordLastChanged,
        EmailVerified = EmailVerified,
        PhoneVerified = PhoneVerified,
        DisableTwoFactorApp = DisableTwoFactorApp,
        DisableTwoFactorSms = DisableTwoFactorSms,
        DisableTwoFactorEmail = DisableTwoFactorEmail,
        RequireMultiFactor = RequireMultiFactor,
        Claims = Claims
    };
}
