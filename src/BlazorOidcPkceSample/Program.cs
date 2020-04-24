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

            //builder.Services.AddSingleton(sp =>
            //{
            //    return new HttpClient(sp.GetRequiredService<AuthorizationMessageHandler>()
            //        .ConfigureHandler(
            //          new[] { "https://localhost:44330/testcorp/dev/blazor_oidcauthcodepkce_sample(login)/" },
            //          scopes: new[] { "openid", "profile", "aspnetcore_api1_sample:some_access" }))
            //    {
            //        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
            //    };
            //});

            var httpClientLogicalName = "BlazorOidcPkceSample.ServerAPI";
            builder.Services.AddHttpClient(httpClientLogicalName, client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            // Supply HttpClient instances that include access tokens when making requests to the server project
            builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(httpClientLogicalName));

            //builder.Services.AddOidcAuthentication(options =>
            //{
            //    // Configure your authentication provider options here.
            //    // For more information, see https://aka.ms/blazor-standalone-auth
            //    options.ProviderOptions.Authority = "https://localhost:44330/testcorp/dev/blazor_oidcauthcodepkce_sample(login)/";
            //    options.ProviderOptions.ClientId = "blazor_oidcauthcodepkce_sample";
            //    options.ProviderOptions.DefaultScopes.Add("aspnetcore_api1_sample:some_access");
            //    options.ProviderOptions.ResponseType = "code";
            //});

            //builder.Services.AddOidcAuthentication(options =>
            //    builder.Configuration.Bind("IdentitySettings", options.ProviderOptions));

            builder.Services.AddOidcAuthentication(options =>
            {
                // Configure your authentication provider options here.
                // For more information, see https://aka.ms/blazor-standalone-auth
                builder.Configuration.Bind("IdentitySettings", options.ProviderOptions);
                options.ProviderOptions.DefaultScopes.Add("aspnetcore_api1_sample:some_access");
                options.ProviderOptions.ResponseType = "code";
            });

            await builder.Build().RunAsync();
        }
    }
}
