using ITfoxtec.Identity.Util;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace FoxIDs.SampleHelperLibrary
{
#if NET472
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
            // Used in .NET Framework
            var cspParameters = new CspParameters(
                /* PROV_RSA_AES */ 24,
                "Microsoft Enhanced RSA and AES Cryptographic Provider",
                Guid.NewGuid().ToString());

            using (RSA rsa = new RSACryptoServiceProvider(2048, cspParameters))
            {
                var certRequest = new CertificateRequest(
                    $"CN={cn}",
                    rsa,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);

                // Explicitly not a CA.
                certRequest.CertificateExtensions.Add(
                    new X509BasicConstraintsExtension(false, false, 0, false));

                certRequest.CertificateExtensions.Add(
                    new X509KeyUsageExtension(
                        X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.NonRepudiation,
                        false));

                var now = DateTimeOffset.UtcNow;
                using (var cert = certRequest.CreateSelfSigned(now.AddDays(-1), now.AddDays(365)))
                {
                    File.WriteAllBytes(PfxFile(path, cn), cert.Export(X509ContentType.Pfx));
                    File.WriteAllBytes(CrtFile(path, cn), cert.Export(X509ContentType.Cert));
                }
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
#endif
}
