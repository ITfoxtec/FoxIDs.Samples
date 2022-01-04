using NetCoreClientGrantConsoleSample.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NetCoreClientGrantConsoleSample.Logic
{
    public class CallApiLogic
    {
        private readonly AppSettings appSettings;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly AccessLogic accessLogic;

        public CallApiLogic(AppSettings appSettings, IHttpClientFactory httpClientFactory, AccessLogic accessLogic)
        {
            this.appSettings = appSettings;
            this.httpClientFactory = httpClientFactory;
            this.accessLogic = accessLogic;
        }

        public async Task CallAspNetCoreApi1SampleAsync()
        {
            var accessToken = await accessLogic.GetAccessTokenAsync();
            Console.WriteLine("\nCalling API 1...");
            using var response = await httpClientFactory.CreateClient().GetAsync(appSettings.AspNetCoreApi1SampleUrl, accessToken, "1234");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API result:\n{result}");
            }
            else
            {
                throw new Exception($"Unable to call API. API URL='{appSettings.AspNetCoreApi1SampleUrl}', StatusCode='{response.StatusCode}'");
            }

        }
    }
}
