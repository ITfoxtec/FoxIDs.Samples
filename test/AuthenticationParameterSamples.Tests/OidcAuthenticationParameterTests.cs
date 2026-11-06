using AspNetCoreOidcAuthCodeAllUpPartiesSample.Controllers;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Net;
using Xunit;

namespace AuthenticationParameterSamples.Tests;

public class OidcAuthenticationParameterTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Login_ConfiguredAuthMethod_SendsQueryOnlyWhenEnabled(bool enabled)
    {
        using var factory = new SampleFactory(enabled);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://app.example.test")
        });

        var response = await client.GetAsync("/Auth/Login");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location;
        Assert.Equal("foxids.example.test", location.Host);
        Assert.Equal("/tenant/environment/app(*)/oauth2/authorize", location.AbsolutePath);
        var query = QueryHelpers.ParseQuery(location.Query);
        Assert.Equal(enabled, query.ContainsKey("auth_method"));
        if (enabled) Assert.Equal("login entra+work", Assert.Single(query["auth_method"]));
        Assert.Equal("app", query["client_id"]);
        Assert.Equal("code", query["response_type"]);
        Assert.Equal("Sales Support", query["departments"]);
        Assert.Equal("profile-test", query["profile_id"]);
        Assert.False(string.IsNullOrWhiteSpace(query["state"]));
    }

    private sealed class SampleFactory(bool enabled) : WebApplicationFactory<AuthController>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.Configure<TelemetryConfiguration>(configuration => configuration.DisableTelemetry = true);
                services.PostConfigure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
                {
                    options.ConfigurationManager = new StaticConfigurationManager<OpenIdConnectConfiguration>(new()
                    {
                        Issuer = "https://foxids.example.test/tenant/environment/",
                        AuthorizationEndpoint = "https://foxids.example.test/tenant/environment/app(*)/oauth2/authorize"
                    });
                });
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureHostConfiguration(configuration => configuration.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["IdentitySettings:FoxIDsAuthority"] = "https://foxids.example.test/tenant/environment/app(*)/",
                ["IdentitySettings:DownParty"] = "app",
                ["IdentitySettings:ClientSecret"] = "test-secret",
                ["IdentitySettings:SendAuthMethodParameter"] = enabled.ToString(),
                ["IdentitySettings:AuthMethod"] = "login entra+work",
                ["IdentitySettings:SendLoginParameters"] = "true",
                ["IdentitySettings:LoginParameterProfileId"] = "profile-test",
                ["IdentitySettings:LoginParameterDepartments"] = "Sales Support",
                ["IdentitySettings:SendImpersonationParameter"] = "false"
            }));
            return base.CreateHost(builder);
        }
    }
}
