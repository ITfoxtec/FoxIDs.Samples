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

                var isRunning = true;
                while (isRunning)
                {
                    Console.WriteLine("Select seed action or click any key to end");
                    Console.WriteLine("C: Create sample configuration (omitting existing parties)");
                    Console.WriteLine("D: Delete sample configuration");

                    var key = Console.ReadKey();
                    Console.WriteLine(string.Empty);
                    Console.WriteLine(string.Empty);

                    try
                    {
                        switch (char.ToLower(key.KeyChar))
                        {
                            case 'c':
                                await serviceProvider.GetService<SampleSeedLogic>().SeedAsync();
                                break;

                            case 'd':
                                await serviceProvider.GetService<SampleSeedLogic>().DeleteAsync();
                                break;

                            default:
                                Console.WriteLine("Canceled");
                                isRunning = false;
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.ToString()}");
                    }

                    Console.WriteLine(string.Empty); 
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.ToString()}");
            }
        }
    }
}
