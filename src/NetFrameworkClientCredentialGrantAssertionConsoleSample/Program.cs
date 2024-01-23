using NetFrameworkClientCredentialGrantAssertionConsoleSample.Logic;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NetFrameworkClientCredentialGrantAssertionConsoleSample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            try
            {
                using(var httpClient = new HttpClient())
                {
                    await CallApiLogic.CallAspNetCoreApi1SampleAsync(httpClient);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex}");
            }
        }
    }
}
