using System.ComponentModel.DataAnnotations;

namespace DirectoryConnectorApiSample.Models.Api;

public abstract class DirectoryUserIdentifierRequest : IValidatableObject
{
    public string Email { get; set; }

    public string Phone { get; set; }

    public string Username { get; set; }

    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (new[] { Email, Phone, Username }.Count(value => !string.IsNullOrWhiteSpace(value)) != 1)
        {
            yield return new ValidationResult(
                $"Exactly one of the fields {nameof(Email)}, {nameof(Phone)} or {nameof(Username)} is required.",
                [nameof(Email), nameof(Phone), nameof(Username)]);
        }
    }
}
