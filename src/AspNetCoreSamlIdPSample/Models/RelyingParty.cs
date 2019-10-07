using System;
using System.Security.Cryptography.X509Certificates;

namespace AspNetCoreSamlIdPSample.Models
{
    public class RelyingParty
    {        
        public string SpMetadata { get; set; }

        public string Issuer { get; set; }

        public Uri SingleSignOnDestination { get; set; }

        public Uri SingleLogoutResponseDestination { get; set; }

        public X509Certificate2 SignatureValidationCertificate { get; set; }
    }
}
