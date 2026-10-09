using FoxIDs.SampleHelperLibrary.Models;
using System.ComponentModel.DataAnnotations;

namespace ExternalClaimsApiSample.Models
{
    public class AppSettings
    {
        public string ApiSecret { get; set; }

        [EnumDataType(typeof(ClaimsFormats))]
        public ClaimsFormats ClaimsFormat { get; set; } = ClaimsFormats.ClaimsList;
    }
}
