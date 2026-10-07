using System.ComponentModel.DataAnnotations;

namespace DirectoryConnectorApiSample.Models.Api;

public class DirectorySynchronisationRequest : DirectoryUserIdentifierRequest
{
    public string DirectoryUserId { get; set; }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext) =>
        string.IsNullOrWhiteSpace(DirectoryUserId) ? base.Validate(validationContext) : [];
}
