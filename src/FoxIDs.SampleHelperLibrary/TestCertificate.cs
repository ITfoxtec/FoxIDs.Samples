using ITfoxtec.Identity.Util;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace FoxIDs.SampleHelperLibrary
{
    public class TestCertificate
    {
        public static X509Certificate2 GetSelfSignedCertificate(string path, string cn)
        {
            var pfxFile = PfxFile(path, cn);

            if (!File.Exists(pfxFile))
            {
                CreateSelfSignedCertificate(path, cn);
            }

            return CertificateUtil.Load(pfxFile);
        }

        private static void CreateSelfSignedCertificate(string path, string cn)
        {
            using (var rsa = RSA.Create(2048))
            {
                var certRequest = new CertificateRequest(
                    $"CN={cn}, O=FoxIDs",
                    rsa,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);

                certRequest.CertificateExtensions.Add(
                    new X509BasicConstraintsExtension(false, false, 0, false));

                certRequest.CertificateExtensions.Add(
                    new X509SubjectKeyIdentifierExtension(certRequest.PublicKey, false));

                certRequest.CertificateExtensions.Add(
                    new X509KeyUsageExtension(
                        X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyAgreement,
                        false));

                var now = DateTimeOffset.UtcNow;
                var cert = certRequest.CreateSelfSigned(now.AddDays(-1), now.AddYears(100));

                File.WriteAllBytes(PfxFile(path, cn), cert.Export(X509ContentType.Pfx));
                File.WriteAllBytes(CrtFile(path, cn), cert.Export(X509ContentType.Cert));
            }
        }

        private static string PfxFile(string path, string cn)
        {
            return Path.Combine(path, $"{cn}.pfx");
        }
        private static string CrtFile(string path, string cn)
        {
            return Path.Combine(path, $"{cn}.crt");
        }
    }
}
