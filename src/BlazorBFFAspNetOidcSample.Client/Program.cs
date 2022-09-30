using BlazorBFFAspNetOidcSample;
using BlazorBFFAspNetOidcSample.Client;
using BlazorBFFAspNetOidcSample.Client.Infrastructure;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorOidcPkceSample
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            // Authentication
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

            builder.Services.AddScoped<AntiforgeryHandler>();

            builder.Services.AddHttpClient(Constants.Client.HttpClientLogicalName, client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
            builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(Constants.Client.HttpClientLogicalName));

            builder.Services.AddHttpClient(Constants.Client.HttpClientSecureLogicalName, client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<AntiforgeryHandler>();
            builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(Constants.Client.HttpClientSecureLogicalName));

            await builder.Build().RunAsync();
        }
    }
}
