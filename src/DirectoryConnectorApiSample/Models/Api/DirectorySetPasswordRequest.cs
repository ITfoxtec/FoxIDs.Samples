using System.ComponentModel.DataAnnotations;

namespace DirectoryConnectorApiSample.Models.Api;

public class DirectorySetPasswordRequest : DirectoryUserIdentifierRequest
{
    [Required]
    public string Password { get; set; }
}
