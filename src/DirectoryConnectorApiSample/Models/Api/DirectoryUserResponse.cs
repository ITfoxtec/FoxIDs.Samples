using System.ComponentModel.DataAnnotations;

namespace DirectoryConnectorApiSample.Models.Api;

public class DirectoryUserResponse : IValidatableObject
{
    [Required]
    public string DirectoryUserId { get; set; }

    public string Email { get; set; }

    public string Phone { get; set; }

    public string Username { get; set; }

    public bool ConfirmAccount { get; set; }

    public bool EmailVerified { get; set; }

    public bool PhoneVerified { get; set; }

    public bool DisableTwoFactorApp { get; set; }

    public bool DisableTwoFactorSms { get; set; }

    public bool DisableTwoFactorEmail { get; set; }

    public bool RequireMultiFactor { get; set; }

    public IEnumerable<ClaimValue> Claims { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Email) && string.IsNullOrWhiteSpace(Phone) && string.IsNullOrWhiteSpace(Username))
        {
            yield return new ValidationResult(
                $"Either the field {nameof(Email)} or the field {nameof(Phone)} or the field {nameof(Username)} is required.",
                [nameof(Email), nameof(Phone), nameof(Username)]);
        }
    }
}
