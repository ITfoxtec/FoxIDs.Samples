using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;

namespace BlazorOidcPkceSample
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            var httpClientLogicalName = "BlazorOidcPkceSample.ServerAPI";
            builder.Services.AddHttpClient(httpClientLogicalName, client => client.BaseAddress = new Uri(builder.Configuration["AppSettings:AuthorizedApi1Url"]))
                .AddHttpMessageHandler(sp =>
                {
                    var handler = sp.GetService<AuthorizationMessageHandler>()
                        .ConfigureHandler(
                            authorizedUrls: new[] { builder.Configuration["AppSettings:AuthorizedApi1Url"] },
                            scopes: new[] { builder.Configuration["AppSettings:Api1Scope"] });
                    return handler;
                });

            // Supply HttpClient instances that include access tokens when making requests to the server project
            builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(httpClientLogicalName));

            builder.Services.AddOidcAuthentication(options =>
            {
                // Configure your authentication provider options here.
                // For more information, see https://aka.ms/blazor-standalone-auth
                builder.Configuration.Bind("IdentitySettings", options.ProviderOptions);
                options.ProviderOptions.DefaultScopes.Add(builder.Configuration["AppSettings:Api1Scope"]);
                options.ProviderOptions.ResponseType = "code";
            });

            await builder.Build().RunAsync();
        }
    }
}
