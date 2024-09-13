using System.ComponentModel.DataAnnotations;

namespace ExternalApiLoginSample.Models.Api
{
    public class AuthenticationRequest
    {
        [Required]
        public ExternalLoginUsernameTypes UsernameType { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        public string SomeCustomId { get; set; }
    }
}
