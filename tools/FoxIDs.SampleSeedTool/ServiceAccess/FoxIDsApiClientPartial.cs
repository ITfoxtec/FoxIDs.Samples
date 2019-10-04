using FoxIDs.SampleSeedTool.Logic;
using FoxIDs.SampleSeedTool.Model;
using System.Net.Http;
using System.Text;

namespace FoxIDs.SampleSeedTool.ServiceAccess
{
    public partial class FoxIDsApiClient
    {
        public FoxIDsApiClient(SeedSettings settings, IHttpClientFactory httpClientFactory, AccessLogic accessLogic) : this(httpClientFactory.CreateClient())
        {
            BaseUrl = settings.FoxIDsApiEndpoint;
            Settings = settings;
            AccessLogic = accessLogic;
        }

        partial void PrepareRequest(HttpClient client, HttpRequestMessage request, StringBuilder urlBuilder)
        {
            _ = client;
            _ = request;

            urlBuilder.Replace("[tenant_name]", Settings.Tenant);
            urlBuilder.Replace("[track_name]", Settings.Track);
        }
    }
}
