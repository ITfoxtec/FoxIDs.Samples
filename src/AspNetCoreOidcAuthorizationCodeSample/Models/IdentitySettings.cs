using SampleHelperLibrary.Models;

namespace AspNetCoreOidcAuthorizationCodeSample.Models
{
    public class IdentitySettings : FoxIDsSettings
    {
        public string ClientId => DownParty;
        public string ClientSecret { get; set; }

        public string FoxIDsLoginUpParty { get; set; }
        public string SamlIdPSampleUpParty { get; set; }
        public string SamlIdPAdfsUpParty { get; set; }
    }
}
