using ITfoxtec.Identity.Helpers;
using NetCoreClientCredentialGrantConsoleSample.Models;
using System;
using System.Threading.Tasks;

namespace NetCoreClientCredentialGrantConsoleSample.Logic
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
                (var accessToken, var expiresIn) = await tokenHelper.GetAccessTokenWithClientCredentialGrantAsync(settings.ClientId, settings.ClientSecret, scope);
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
