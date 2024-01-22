using FoxIDs.SampleHelperLibrary.Models;

namespace BlazorServerOidcSample.Models
{
    public class IdentitySettings : FoxIDsSettings
    {
        public string ClientId => DownParty;
        public string ClientSecret { get; set; }

        public string FoxIDsUpParty { get; set; }
    }
}
