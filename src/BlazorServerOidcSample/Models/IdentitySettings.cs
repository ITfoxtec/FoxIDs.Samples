using FoxIDs.SampleHelperLibrary.Models;

namespace BlazorServerOidcSample.Models
{
    public class IdentitySettings : FoxIDsSettings
    {
        public string ClientId => DownParty;
        public string ClientSecret { get; set; }

        public string FoxIDsLoginUpParty { get; set; }
        public string ParallelFoxIDsUpParty { get; set; }
        public string IdentityServerUpParty { get; set; }
        public string AzureAdUpParty { get; set; }
        public string SamlIdPSampleUpParty { get; set; }
        public string SamlAdfsUpParty { get; set; }
        public string SamlNemLoginUpParty { get; set; }
    }
}
