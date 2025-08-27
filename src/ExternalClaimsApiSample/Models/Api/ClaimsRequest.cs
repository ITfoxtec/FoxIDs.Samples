using System.ComponentModel.DataAnnotations;

namespace ExternalClaimsApiSample.Models.Api
{
    public class ClaimsRequest
    {
        [Required]
        public IEnumerable<ClaimValue> Claims { get; set; }
    }
}
