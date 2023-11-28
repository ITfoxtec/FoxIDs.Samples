using ITfoxtec.Identity.Saml2;

namespace AspNetCoreSamlIdPSample.Models
{
    public class Saml2ConfigurationIdP : Saml2Configuration
    {
        public string TokenExchangeClientCertificateThumbprint { get; set; }
        // Thumbprint OR by file
        public string TokenExchangeClientCertificateFile { get; set; }
        public string TokenExchangeClientCertificatePassword { get; set; }
    }
}
