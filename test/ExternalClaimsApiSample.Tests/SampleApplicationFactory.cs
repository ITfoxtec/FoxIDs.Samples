using FoxIDs.SampleHelperLibrary.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Net.Http.Headers;
using System.Text;

namespace ExternalClaimsApiSample.Tests;

internal sealed class SampleApplicationFactory(ClaimsFormats claimsFormat = ClaimsFormats.ClaimsList) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseEnvironment("Testing");

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(configuration => configuration.AddInMemoryCollection(
            new Dictionary<string, string>
            {
                ["AppSettings:ApiSecret"] = "test-secret",
                ["AppSettings:ClaimsFormat"] = claimsFormat.ToString()
            }));
        return base.CreateHost(builder);
    }

    public HttpClient CreateApiClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes("external_claims:test-secret")));
        return client;
    }
}
