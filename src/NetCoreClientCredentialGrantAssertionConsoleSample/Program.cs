using NetCoreClientCredentialGrantAssertionConsoleSample.Infrastructure;
using NetCoreClientCredentialGrantAssertionConsoleSample.Logic;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Logging;

namespace NetCoreClientCredentialGrantAssertionConsoleSample
{
    class Program
    {
        static void Main(string[] args)
        {
            IdentityModelEventSource.ShowPII = true; //To show detail of error and see the problem

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
