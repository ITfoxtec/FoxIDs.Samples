using System.ComponentModel.DataAnnotations;

namespace ExternalLoginApiSample.Models.Api
{
    public class ClaimValue
    {
        [Required]
        public string Type { get; set; }

        [Required]
        public string Value { get; set; }
    }
}
