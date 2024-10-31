using System.ComponentModel.DataAnnotations;

namespace ExternalApiLoginSample.Models.Api
{
    public class ClaimValue
    {
        [Required]
        public string Type { get; set; }

        [Required]
        public string Value { get; set; }
    }
}
