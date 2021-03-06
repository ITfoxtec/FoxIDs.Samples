﻿using ITfoxtec.Identity.Helpers;
using FoxIDs.SampleSeedTool.Models;
using System;
using System.Threading.Tasks;

namespace FoxIDs.SampleSeedTool.Logic
{
    public class AccessLogic
    {
        private readonly SeedSettings settings;
        private readonly TokenHelper tokenHelper;
        private string accessTokenCache;
        private long cacheExpiresAt;

        public AccessLogic(SeedSettings settings, TokenHelper tokenHelper)
        {
            this.settings = settings;
            this.tokenHelper = tokenHelper;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            if (cacheExpiresAt < DateTimeOffset.UtcNow.AddSeconds(-5).ToUnixTimeSeconds())
            {
                Console.WriteLine("\t\tGetting sample seed client access token.");
                (var accessToken, var expiresIn) = await tokenHelper.GetAccessTokenWithClientCredentialsAsync(settings.ClientId, settings.ClientSecret, settings.RedirectUri, "foxids_control_api:foxids:tenant");
                accessTokenCache = accessToken;
                cacheExpiresAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + expiresIn;
                Console.WriteLine($"\t\tAccess token: {accessToken.Substring(0, 40)}...");
            }
            return accessTokenCache;
        }
    }
}
