using System.ComponentModel.DataAnnotations;

namespace DirectoryConnectorApiSample.Models.Api;

public class DirectoryCreateUserResponse
{
    [Required]
    public string DirectoryUserId { get; set; }
}
