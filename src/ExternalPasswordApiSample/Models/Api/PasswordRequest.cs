using ITfoxtec.Identity;
using System.ComponentModel.DataAnnotations;

namespace ExternalPasswordApiSample.Models.Api;

public class PasswordRequest : IValidatableObject
{
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Email.IsNullOrWhiteSpace() && Phone.IsNullOrWhiteSpace() && Username.IsNullOrWhiteSpace())
        {
            yield return new ValidationResult("At least one user identifier (email, phone, username) must be provided.");
        }
    }
}