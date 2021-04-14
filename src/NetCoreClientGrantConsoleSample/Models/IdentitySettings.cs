using FoxIDs.SampleHelperLibrary.Models;

namespace NetCoreClientGrantConsoleSample.Models
{
    public class IdentitySettings : FoxIDsSettings
    {
        public string ClientId => DownParty;
        public string ClientSecret { get; set; }
        public string RedirectUri { get; set; }
    }
}
