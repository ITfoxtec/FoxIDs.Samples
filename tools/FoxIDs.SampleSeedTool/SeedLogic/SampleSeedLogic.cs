using FoxIDs.SampleSeedTool;
using FoxIDs.SampleSeedTool.Logic;
using FoxIDs.SampleSeedTool.Model;
using FoxIDs.SampleSeedTool.ServiceAccess;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using UrlCombineLib;

namespace FoxIDs.SampleSeedTool.SeedLogic
{
    public class SampleSeedLogic
    {
        private readonly SeedSettings settings;
        private readonly FoxIDsApiClient foxIDsApiClient;

        public SampleSeedLogic(SeedSettings settings, FoxIDsApiClient foxIDsApiClient)
        {
            this.settings = settings;
            this.foxIDsApiClient = foxIDsApiClient;
        }

        public string TrackApiEndpoint => UrlCombine.Combine(settings.FoxIDsApiEndpoint, settings.Tenant, settings.Track);

        public async Task SeedAsync()
        {
            Console.WriteLine("Create sample configuration");

            await CreateAspNetCoreApi1SampleAsync();


            Console.WriteLine(string.Empty);
            Console.WriteLine($"Sample configuration created");
        }

        private async Task CreateAspNetCoreApi1SampleAsync()
        {
            Console.WriteLine("Creating AspNetCoreApi1Sample");

            await foxIDsApiClient.PostOAuthDownPartyAsync(new OAuthDownParty
            {
                Name = "aspnetcore_api1_sample",
                Type = "OAuth2",
                //Allow_up_parties = new[] { new PartyDataElement { Type = "Oidc", Id = "" } },                
                Resource = new OAuthDownResource
                {
                    Scopes = new[] { "some_access" }
                }
            });

            Console.WriteLine($"AspNetCoreApi1Sample created");
        }

        //private async Task SaveOAuthDownPartyDocumentAsync(OAuthDownParty oauthDownParty)
        //{
        //    var accessToken = await accessLogic.GetAccessTokenAsync();

        //    var client = httpClientFactory.CreateClient();
        //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        //    using (var response = await client.PostJsonAsync(UrlCombine.Combine(TrackApiEndpoint, "!oauthdownparty"), oauthDownParty))
        //    {
        //        await response.ValidateResponseAsync();
        //    }
        //}
    }
}
