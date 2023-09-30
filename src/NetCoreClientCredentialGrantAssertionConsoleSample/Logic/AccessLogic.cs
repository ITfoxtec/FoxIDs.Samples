using ITfoxtec.Identity.Helpers;
using ITfoxtec.Identity.Util;
using NetCoreClientCredentialGrantAssertionConsoleSample.Models;
using System;
using System.Threading.Tasks;

namespace NetCoreClientCredentialGrantAssertionConsoleSample.Logic
{
    public class AccessLogic
    {
        private readonly IdentitySettings settings;
        private readonly TokenHelper tokenHelper;
        private string accessTokenCache;
        private long cacheExpiresAt;

        public AccessLogic(IdentitySettings settings, TokenHelper tokenHelper)
        {
            this.settings = settings;
            this.tokenHelper = tokenHelper;
        }

        public async Task<string> GetAccessTokenAsync(string scope)
        {
            if (cacheExpiresAt < DateTimeOffset.UtcNow.AddSeconds(-5).ToUnixTimeSeconds())
            {
                Console.WriteLine("Acquire access token...");
                var clientCertificate = CertificateUtil.Load(settings.ClientCertificateFile, settings.ClientCertificatePassword);
                (var accessToken, var expiresIn) = await tokenHelper.GetAccessTokenWithAssertionClientCredentialGrantAsync(clientCertificate, settings.ClientId, scope);
                accessTokenCache = accessToken;
                cacheExpiresAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + expiresIn.Value;
                Console.WriteLine($"Access token: {accessToken.Substring(0, 40)}...");
            }
            else
            {
                Console.WriteLine($"Access token from cache: {accessTokenCache.Substring(0, 40)}...");
            }
            return accessTokenCache;
        }
    }
}
