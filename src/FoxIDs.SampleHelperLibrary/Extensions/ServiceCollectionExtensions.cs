using FoxIDs.SampleHelperLibrary.Models;
using Microsoft.Extensions.Configuration;
using FoxIDs.SampleHelperLibrary.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// TEST ONLY: Keeps authentication tickets in server memory and only a session reference in the cookie.
        /// Use an ITicketStore backed by shared storage or a database in production.
        /// </summary>
        public static IServiceCollection AddTestInMemoryTicketStore(this IServiceCollection services,
            string authenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme)
        {
            services.AddMemoryCache();
            services.TryAddSingleton<TestInMemoryTicketStore>();
            services.AddOptions<CookieAuthenticationOptions>(authenticationScheme)
                .Configure<TestInMemoryTicketStore>((options, ticketStore) => options.SessionStore = ticketStore);
            return services;
        }

        public static T BindConfig<T>(this IServiceCollection services, IConfiguration configuration, string key) where T : class, new()
        {
            var settings = new T();
            configuration.Bind(key, settings);
            services.AddSingleton(settings);
            if (settings is LibrarySettings librarySettings)
            {
                services.AddSingleton(librarySettings);
            }
            return settings;
        }
    }
}
