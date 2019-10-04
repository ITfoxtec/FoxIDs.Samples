using FoxIDs.SampleSeedTool.Logic;
using FoxIDs.SampleSeedTool.Model;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace FoxIDs.SampleSeedTool.ServiceAccess
{
    public abstract class FoxIDsApiClientBase
    {
        protected SeedSettings Settings { get; set; }
        protected AccessLogic AccessLogic { get; set; }

        protected async Task<HttpRequestMessage> CreateHttpRequestMessageAsync(CancellationToken cancellationToken)
        {
            _ = cancellationToken;

            var request = new HttpRequestMessage();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await AccessLogic.GetAccessTokenAsync());
            return request;
        }
    }
}
