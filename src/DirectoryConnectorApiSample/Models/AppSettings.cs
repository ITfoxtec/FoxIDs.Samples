using FoxIDs.SampleHelperLibrary.Models;
using System.ComponentModel.DataAnnotations;

namespace DirectoryConnectorApiSample.Models;

public class AppSettings : LibrarySettings
{
    public string ApiSecret { get; set; }

    [EnumDataType(typeof(ClaimsFormats))]
    public ClaimsFormats ClaimsFormat { get; set; } = ClaimsFormats.ClaimsList;
}
