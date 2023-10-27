using NetCoreClientCredentialGrantAssertionConsoleSample.Infrastructure;
using NetCoreClientCredentialGrantAssertionConsoleSample.Logic;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace NetCoreClientCredentialGrantAssertionConsoleSample
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            try
            {
                var serviceProvider = new StartupConfigure().ConfigureServices();

                // An access token is subsequently acquired using OAuth 2.0 Client Credentials Grant.

                // Call sample API 1. 
                await serviceProvider.GetService<CallApiLogic>().CallAspNetCoreApi1SampleAsync();
                // Call sample API which support Two IdPs. 
                //await serviceProvider.GetService<CallApiLogic>().CallAspNetCoreApiOAuthTwoIdPsSampleAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex}");
            }
        }
    }
}
