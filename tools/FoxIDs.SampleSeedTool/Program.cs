using System;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using FoxIDs.SampleSeedTool.Infrastructure;
using FoxIDs.SampleSeedTool.SeedLogic;

namespace FoxIDs.SampleSeedTool
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

                await serviceProvider.GetService<SampleSeedLogic>().SeedAsync();

                Console.WriteLine("Click any key to end...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.ToString()}");
            }
        }
    }
}
