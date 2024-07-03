using System.ComponentModel.DataAnnotations;

namespace ExternalLoginApiSample.Models.Api
{
    public class AuthenticationResponse
    {
        [Required]
        public IEnumerable<ClaimValue> Claims { get; set; }
    }
}
