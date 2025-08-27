using ITfoxtec.Identity.Helpers;
using FoxIDs.ControlApiSample.Models;
using System;
using System.Threading.Tasks;

namespace FoxIDs.ControlApiSample.Logic
{
    public class AccessLogic
    {
        private readonly ApiSampleSettings settings;
        private readonly TokenHelper tokenHelper;
        private string accessTokenCache;
        private long cacheExpiresAt;

        public AccessLogic(ApiSampleSettings settings, TokenHelper tokenHelper)
        {
            this.settings = settings;
            this.tokenHelper = tokenHelper;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            if (cacheExpiresAt < DateTimeOffset.UtcNow.AddSeconds(-5).ToUnixTimeSeconds())
            {
                Console.WriteLine("\t\tAcquire sample seed client access token...");
                (var accessToken, var expiresIn) = await tokenHelper.GetAccessTokenWithClientCredentialGrantAsync(settings.ClientId, settings.ClientSecret, settings.Scope);
                accessTokenCache = accessToken;
                cacheExpiresAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + expiresIn.Value;
                Console.WriteLine($"\t\tAccess token: {accessToken.Substring(0, 40)}...");
            }
            return accessTokenCache;
        }
    }
}
