using FoxIDs.ControlApiSample.Logic;
using FoxIDs.ControlApiSample.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace FoxIDs.ControlApiSample.ServiceAccess
{
    public abstract class FoxIDsApiClientBase
    {
        protected ApiSampleSettings Settings { get; set; }
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
