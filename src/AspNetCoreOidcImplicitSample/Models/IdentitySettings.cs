using FoxIDs.SampleHelperLibrary.Models;

namespace AspNetCoreOidcImplicitSample.Models
{
    public class IdentitySettings : FoxIDsSettings
    {
        public string ClientId => DownParty;
        public string ClientSecret { get; set; }

        public string FoxIDsLoginUpParty { get; set; }
        public string ParallelFoxIDsUpParty { get; set; }
        public string IdentityServerUpParty { get; set; }
        public string SamlIdPSampleUpParty { get; set; }
        public string SamlIdPAdfsUpParty { get; set; }
    }
}
