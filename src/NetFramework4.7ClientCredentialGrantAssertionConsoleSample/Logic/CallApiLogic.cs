using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace NetFrameworkClientCredentialGrantAssertionConsoleSample.Logic
{
    public class CallApiLogic
    {
        public static async Task CallAspNetCoreApi1SampleAsync(HttpClient httpClient)
        {
            var accessToken = await AccessLogic.GetAccessTokenAsync(httpClient, "aspnetcore_api1_sample:some_access");
            Console.WriteLine("\nCalling API 1...");

            var aspNetCoreApi1SampleUrl = ConfigurationManager.AppSettings["AspNetCoreApi1SampleUrl"];
            var api1CallValue = "4567";
            var api1Url = $"{aspNetCoreApi1SampleUrl}/{api1CallValue}";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using (var response = await httpClient.GetAsync(api1Url))
            {
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API result:\n{result}");
                }
                else
                {
                    throw new Exception($"Unable to call API. API URL='{aspNetCoreApi1SampleUrl}', StatusCode='{response.StatusCode}'");
                }
            }
        }
    }
}
