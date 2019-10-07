using FoxIDs.SampleSeedTool.Logic;
using FoxIDs.SampleSeedTool.Model;
using FoxIDs.SampleSeedTool.SeedLogic;
using FoxIDs.SampleSeedTool.ServiceAccess;
using ITfoxtec.Identity;
using ITfoxtec.Identity.Discovery;
using ITfoxtec.Identity.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Net.Http;
using UrlCombineLib;

namespace FoxIDs.SampleSeedTool.Infrastructure
{
    public class StartupConfigure
    {
        private ServiceCollection services;

        public IServiceProvider ConfigureServices()
        {
            services = new ServiceCollection();

            AddConfiguration();
            AddInfrastructure(services);
            AddLogic(services);
            AddSeedLogic(services);

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }

        private static void AddSeedLogic(ServiceCollection services)
        {
            services.AddTransient<SampleSeedLogic>();
        }

        private static void AddLogic(ServiceCollection services)
        {
            services.AddSingleton<AccessLogic>();
        }

        private static void AddInfrastructure(ServiceCollection services)
        {
            services.AddHttpClient();

            services.AddTransient<TokenHelper>();
            services.AddSingleton(serviceProvider =>
            {
                var settings = serviceProvider.GetService<SeedSettings>();
                var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

                return new OidcDiscoveryHandler(httpClientFactory, UrlCombine.Combine(settings.Authority, IdentityConstants.OidcDiscovery.Path));
            });

            services.AddTransient<FoxIDsApiClient>();
        }

        private void AddConfiguration()
        {
            var builder = new ConfigurationBuilder()
                                 .SetBasePath(Directory.GetCurrentDirectory())
                                 .AddJsonFile("appsettings.json")
                                 .AddJsonFile("appsettings.Development.json", optional: true);

            var configuration = builder.Build();
            var seedSettings = new SeedSettings();
            configuration.Bind(nameof(SeedSettings), seedSettings);
            services.AddSingleton(seedSettings);
        }
    }
}
