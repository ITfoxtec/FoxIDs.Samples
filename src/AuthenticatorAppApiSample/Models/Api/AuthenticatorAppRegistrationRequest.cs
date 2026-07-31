using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AuthenticatorAppApiSample.Models.Api;

public class AuthenticatorAppRegistrationRequest
{
    [Required]
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [Required]
    [MaxLength(36)]
    [JsonPropertyName("registration_id")]
    public string RegistrationId { get; set; }

    [Required]
    [JsonPropertyName("user_id")]
    public string UserId { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("phone")]
    public string Phone { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }
}
