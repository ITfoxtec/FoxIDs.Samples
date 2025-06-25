using System.ComponentModel.DataAnnotations;

namespace ExternalExtendedUiApiSample.Models.Api
{
    public class ElementValue
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Type { get; set; }

        public string ClaimType { get; set; }

        public string Value { get; set; }
    }
}
