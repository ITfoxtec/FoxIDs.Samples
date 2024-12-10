using FoxIDs.ControlApiSample.Logic;
using FoxIDs.ControlApiSample.Models;
using System.Net.Http;
using System.Text;

namespace FoxIDs.ControlApiSample.ServiceAccess
{
    public partial class FoxIDsApiClient
    {
        public FoxIDsApiClient(ApiSampleSettings settings, IHttpClientFactory httpClientFactory, AccessLogic accessLogic) : this(httpClientFactory.CreateClient())
        {
            BaseUrl = settings.FoxIDsConsolApiEndpoint;
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
