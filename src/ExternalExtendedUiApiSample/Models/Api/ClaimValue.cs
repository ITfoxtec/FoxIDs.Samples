using System.ComponentModel.DataAnnotations;

namespace ExternalExtendedUiApiSample.Models.Api
{
    public class ClaimValue
    {
        [Required]
        public string Type { get; set; }

        [Required]
        public string Value { get; set; }
    }
}
