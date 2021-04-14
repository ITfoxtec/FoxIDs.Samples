using NetCoreClientGrantConsoleSample.Infrastructure;
using NetCoreClientGrantConsoleSample.Logic;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace NetCoreClientGrantConsoleSample
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

                // Call sample API 1. 
                // An access token is subsequently acquire using OAuth 2.0 Client Credentials Grant.
                await serviceProvider.GetService<CallApiLogic>().CallAspNetCoreApi1SampleAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex}");
            }
        }
    }
}
