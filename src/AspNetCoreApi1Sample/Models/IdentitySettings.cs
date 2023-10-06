using FoxIDs.SampleHelperLibrary.Models;

namespace AspNetCoreApi1Sample.Models
{
    public class IdentitySettings : FoxIDsSettings
    {
        public string ClientId => DownParty;
        public string ResourceId => DownParty;

        public string TokenExchangeClientCertificateFile { get; set; }
        public string TokenExchangeClientCertificatePassword { get; set; }
    }
}
