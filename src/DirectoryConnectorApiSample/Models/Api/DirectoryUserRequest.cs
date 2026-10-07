using System.ComponentModel.DataAnnotations;

namespace DirectoryConnectorApiSample.Models.Api;

public abstract class DirectoryUserRequest
{
    [Required]
    public string DirectoryUserId { get; set; }
}
