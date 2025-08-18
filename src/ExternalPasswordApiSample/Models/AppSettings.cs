using FoxIDs.SampleHelperLibrary.Models;

namespace ExternalPasswordApiSample.Models;

public class AppSettings : LibrarySettings
{
    public string ApiSecret { get; set; }
}
