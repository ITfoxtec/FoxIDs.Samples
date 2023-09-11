using FoxIDs.SampleHelperLibrary.Models;

namespace NetCoreClientGrantConsoleSample.Models
{
    public class IdentitySettings : FoxIDsSettings
    {
        public string ClientId => DownParty;
        public string ClientCertificateFile { get; set; }
        public string ClientCertificatePassword { get; set; }
    }
}
