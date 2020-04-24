using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace System.Net.Http
{
    public static class HttpClientExtensions
    {        
        public static Task<HttpResponseMessage> GetAsync(this HttpClient client, string requestUri, string token)
        {
            client.SetToken(token);
            return client.GetAsync(requestUri);
        }

        public static Task<HttpResponseMessage> GetAsync(this HttpClient client, string requestUri, string token, string id)
        {
            client.SetToken(token);
            return client.GetAsync($"{requestUri}{(requestUri.EndsWith("/", StringComparison.InvariantCultureIgnoreCase) ? "" : "/")}{id}");
        }

        private static void SetToken(this HttpClient client, string token)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
