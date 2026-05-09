using System.ComponentModel.DataAnnotations;

namespace DirectoryConnectorApiSample.Models.Api;

public abstract class DirectoryUserIdentifierRequest : IValidatableObject
{
    public string DirectoryUserId { get; set; }

    public string Email { get; set; }

    public string Phone { get; set; }

    public string Username { get; set; }

    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Email) && string.IsNullOrWhiteSpace(Phone) && string.IsNullOrWhiteSpace(Username))
        {
            yield return new ValidationResult(
                $"Either the field {nameof(Email)} or the field {nameof(Phone)} or the field {nameof(Username)} is required.",
                [nameof(Email), nameof(Phone), nameof(Username)]);
        }
    }
}
