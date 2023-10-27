using FoxIDs.SampleHelperLibrary.Models;

namespace NetCoreClientCredentialGrantConsoleSample.Models
{
    public class IdentitySettings : FoxIDsSettings
    {
        public string ClientId => DownParty;
        public string ClientSecret { get; set; }
    }
}
