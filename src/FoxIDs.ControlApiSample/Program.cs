using System;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using FoxIDs.ControlApiSample.Infrastructure;
using FoxIDs.ControlApiSample.Logic;

namespace FoxIDs.ControlApiSample
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
                    Console.WriteLine("Select API action or click any key to end");
                    Console.WriteLine("C: Change users password");

                    var key = Console.ReadKey();
                    Console.WriteLine(string.Empty);
                    Console.WriteLine(string.Empty);

                    try
                    {
                        switch (char.ToLower(key.KeyChar))
                        {
                            case 'c':
                                await serviceProvider.GetService<ApiSampleLogic>().ChangeUsersPasswordAsync();
                                break;

                            default:
                                Console.WriteLine("Canceled");
                                isRunning = false;
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex}");
                    }

                    Console.WriteLine(string.Empty); 
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }
        }
    }
}
