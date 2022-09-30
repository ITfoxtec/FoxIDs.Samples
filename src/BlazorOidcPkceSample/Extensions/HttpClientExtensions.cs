using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorOidcPkceSample
{
    public static class HttpClientExtensions
    {
        public static Task<string> GetStringAsync(this HttpClient client, string requestUri, string id)
        {
            return client.GetStringAsync($"{requestUri}{(requestUri.EndsWith("/", StringComparison.InvariantCultureIgnoreCase) ? "" : "/")}{id}");
        }
    }
}
