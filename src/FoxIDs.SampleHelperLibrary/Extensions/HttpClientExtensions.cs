using ITfoxtec.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace System.Net.Http
{
    public static class HttpClientExtensions
    {
        static JsonSerializerSettings serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public static Task<HttpResponseMessage> GetAsync(this HttpClient client, string requestUri, string token)
        {
            client.SetAuthorizationHeaderBearer(token);
            return client.GetAsync(requestUri);
        }

        public static Task<HttpResponseMessage> GetAsync(this HttpClient client, string requestUri, string token, string id)
        {
            client.SetAuthorizationHeaderBearer(token);
            return client.GetAsync($"{requestUri}{(requestUri.EndsWith("/", StringComparison.InvariantCultureIgnoreCase) ? "" : "/")}{id}");
        }

        public static Task<HttpResponseMessage> PostJsonAsync(this HttpClient client, string requestUri, string token, object data)
        {
            client.SetAuthorizationHeaderBearer(token);
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = new StringContent(JsonConvert.SerializeObject(data, serializerSettings), Encoding.UTF8, MediaTypeNames.Application.Json);
            return client.SendAsync(request);
        }

        public static Task<HttpResponseMessage> UpdateJsonAsync(this HttpClient client, string requestUri, string token, object data)
        {
            client.SetAuthorizationHeaderBearer(token);
            var request = new HttpRequestMessage(HttpMethod.Put, requestUri);
            request.Content = new StringContent(JsonConvert.SerializeObject(data, serializerSettings), Encoding.UTF8, MediaTypeNames.Application.Json);
            return client.SendAsync(request);
        }

        public static Task<HttpResponseMessage> PatchJsonAsync(this HttpClient client, string requestUri, string token, object data)
        {
            client.SetAuthorizationHeaderBearer(token);
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
            request.Content = new StringContent(JsonConvert.SerializeObject(data, serializerSettings), Encoding.UTF8, MediaTypeNames.Application.Json);
            return client.SendAsync(request);
        }

        public static Task<HttpResponseMessage> DeleteAsync(this HttpClient client, string requestUri, string token, string id)
        {
            client.SetAuthorizationHeaderBearer(token);
            return client.DeleteAsync($"{requestUri}{(requestUri.EndsWith("/", StringComparison.InvariantCultureIgnoreCase) ? "" : "/")}{id}");
        }
    }
}
