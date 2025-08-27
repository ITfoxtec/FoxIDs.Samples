using FoxIDs.SampleHelperLibrary.Models;

namespace AspNetCoreApi1Sample.Models
{
    public class IdentitySettings : FoxIDsSettings
    {
        public string ClientId => DownParty;
        public string ResourceId => DownParty;

        public string TokenExchangeClientCertificateThumbprint { get; set; }
        // Thumbprint OR by file
        public string TokenExchangeClientCertificateFile { get; set; }
        public string TokenExchangeClientCertificatePassword { get; set; }

        public string RequestApi2Scope { get; set; }
    }
}
