using FoxIDs.SampleSeedTool.Logic;
using FoxIDs.SampleSeedTool.Model;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FoxIDs.SampleSeedTool.ServiceAccess
{
    public partial class FoxIDsApiClient
    {
        private readonly SeedSettings settings;
        private readonly AccessLogic accessLogic;

        public FoxIDsApiClient(SeedSettings settings, IHttpClientFactory httpClientFactory, AccessLogic accessLogic) : this(httpClientFactory.CreateClient())
        {
            BaseUrl = settings.FoxIDsApiEndpoint;
            this.settings = settings;
            this.accessLogic = accessLogic;
        }

        partial void PrepareRequest(HttpClient client, HttpRequestMessage request, StringBuilder urlBuilder)
        {
            PrepareRequestInternal(request, urlBuilder).GetAwaiter().GetResult();
        }

        protected virtual async Task PrepareRequestInternal(HttpRequestMessage request, StringBuilder urlBuilder)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await accessLogic.GetAccessTokenAsync());

            urlBuilder.Replace("[tenant_name]", settings.Tenant);
            urlBuilder.Replace("[track_name]", settings.Track);
        }
    }
}