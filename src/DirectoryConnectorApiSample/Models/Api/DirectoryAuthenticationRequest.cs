using System.ComponentModel.DataAnnotations;

namespace DirectoryConnectorApiSample.Models.Api;

public class DirectoryAuthenticationRequest : DirectoryUserIdentifierRequest
{
    [Required]
    public string Password { get; set; }
}
