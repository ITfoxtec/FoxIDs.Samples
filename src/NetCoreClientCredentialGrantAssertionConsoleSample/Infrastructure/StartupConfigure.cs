using ITfoxtec.Identity;
using ITfoxtec.Identity.Discovery;
using ITfoxtec.Identity.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetCoreClientCredentialGrantAssertionConsoleSample.Logic;
using NetCoreClientCredentialGrantAssertionConsoleSample.Models;
using System;
using System.IO;
using System.Net.Http;
using ITfoxtec.Identity.Util;

namespace NetCoreClientCredentialGrantAssertionConsoleSample.Infrastructure
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

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }

        private static void AddLogic(ServiceCollection services)
        {
            services.AddSingleton<AccessLogic>();
            services.AddTransient<CallApiLogic>();
        }

        private static void AddInfrastructure(ServiceCollection services)
        {
            services.AddHttpClient();

            services.AddTransient<TokenHelper>();
            services.AddSingleton(serviceProvider =>
            {
                var settings = serviceProvider.GetService<IdentitySettings>();
                var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

                return new OidcDiscoveryHandler(httpClientFactory, UrlCombine.Combine(settings.AuthorityWithoutUpParty, IdentityConstants.OidcDiscovery.Path));
            });
        }

        private void AddConfiguration()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
                                 .SetBasePath(Directory.GetCurrentDirectory())
                                 .AddJsonFile("appsettings.json")
                                 .AddJsonFile($"appsettings.{environment}.json", optional: true)
                                 .AddEnvironmentVariables();

            var configuration = builder.Build();

            services.BindConfig<IdentitySettings>(configuration, nameof(IdentitySettings));
            services.BindConfig<AppSettings>(configuration, nameof(AppSettings));
        }
    }
}
