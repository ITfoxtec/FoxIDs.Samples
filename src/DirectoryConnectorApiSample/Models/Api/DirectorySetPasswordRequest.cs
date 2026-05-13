using System.ComponentModel.DataAnnotations;

namespace DirectoryConnectorApiSample.Models.Api;

public class DirectorySetPasswordRequest : DirectoryUserIdentifierRequest
{
    protected override bool RequireDirectoryUserId => true;

    [Required]
    public string Password { get; set; }
}
