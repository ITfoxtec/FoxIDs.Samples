using System.ComponentModel.DataAnnotations;

namespace DirectoryConnectorApiSample.Models.Api;

public class DirectorySetPasswordRequest : DirectoryUserRequest
{
    [Required]
    public string Password { get; set; }
}
