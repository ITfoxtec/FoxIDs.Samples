using System.ComponentModel.DataAnnotations;

namespace ExternalExtendedUiApiSample.Models.Api
{
    public class ElementError
    {
        [Required]
        public string Name { get; set; }

        public string UiErrorMessage { get; set; }
    }
}
