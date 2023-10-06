using FoxIDs.SampleHelperLibrary.Models;

namespace NetCoreClientCredentialGrantAssertionConsoleSample.Models
{
    public class IdentitySettings : FoxIDsSettings
    {
        public string ClientId => DownParty;
        public string ClientCertificateFile { get; set; }
        public string ClientCertificatePassword { get; set; }
    }
}
