using System.Net;
using System.Security.Claims;
using FoxIDs.SampleHelperLibrary.Identity;
using ITfoxtec.Identity.Saml2.MvcCore.Configuration;
using ITfoxtec.Identity.Saml2.Schemas;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace FoxIDs.SampleHelperLibrary.Tests;

public class AuthenticationSessionTests
{
    private static readonly string LargeClaim = new('c', 16000);
    private static readonly string AccessToken = new('a', 8000);
    private static readonly string RefreshToken = new('r', 8000);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LargeTicketsUseOneSmallCookieAndKeepClaimsAndTokensOnServer(bool saml)
    {
        using var host = await CreateHost(new TestClock(), saml);
        var server = host.GetTestServer();
        using var client = server.CreateClient();
        var response = await client.GetAsync("/login");
        var header = Assert.Single(response.Headers.GetValues("Set-Cookie"));
        Assert.True(header.Length < 1024, $"Expected a small cookie, got {header.Length} characters.");
        Assert.DoesNotContain("chunks-", header);

        var options = GetCookieOptions(server, saml);
        var cookie = header.Split(';')[0];
        var reference = options.TicketDataFormat.Unprotect(cookie[(cookie.IndexOf('=') + 1)..]);
        Assert.NotNull(reference);
        var sessionId = Assert.Single(reference.Principal.Claims).Value;
        Assert.Empty(reference.Properties.GetTokens());
        var ticket = await options.SessionStore!.RetrieveAsync(sessionId);
        Assert.NotNull(ticket);
        Assert.Equal(LargeClaim, ticket.Principal.FindFirst("large_claim")!.Value);
        Assert.Equal(AccessToken, ticket.Properties.GetTokenValue("access_token"));
        Assert.Equal(RefreshToken, ticket.Properties.GetTokenValue("refresh_token"));
        Assert.Equal("saved-id-token", ticket.Properties.GetTokenValue("id_token"));

        client.DefaultRequestHeaders.Add("Cookie", cookie);
        Assert.Equal($"{AccessToken}|{RefreshToken}|16000", await client.GetStringAsync("/session"));

        await client.GetAsync("/logout");
        Assert.Null(await options.SessionStore.RetrieveAsync(sessionId));
        // Replaying the previous cookie after logout must not authenticate.
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/session")).StatusCode);
    }

    [Fact]
    public async Task RenewalPersistsRefreshedTokensAndExtendsTheStoredSession()
    {
        var clock = new TestClock();
        using var host = await CreateHost(clock);
        var server = host.GetTestServer();
        using var client = server.CreateClient();
        var response = await client.GetAsync("/login");
        client.DefaultRequestHeaders.Add("Cookie", Assert.Single(response.Headers.GetValues("Set-Cookie")).Split(';')[0]);

        clock.Advance(TimeSpan.FromMinutes(6));
        var refreshResponse = await client.GetAsync("/refresh");
        Assert.Equal("renewed-access-token|renewed-refresh-token|16000", await refreshResponse.Content.ReadAsStringAsync());
        Assert.True(Assert.Single(refreshResponse.Headers.GetValues("Set-Cookie")).Length < 1024);

        clock.Advance(TimeSpan.FromMinutes(5));
        Assert.Equal("renewed-access-token|renewed-refresh-token|16000", await client.GetStringAsync("/session"));

        clock.Advance(TimeSpan.FromMinutes(6));
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/session")).StatusCode);
    }

    [Fact]
    public async Task ExpiredTicketsAreRemovedFromTheStore()
    {
        var clock = new TestClock();
        using var cache = new MemoryCache(new MemoryCacheOptions { Clock = clock });
        var store = new TestInMemoryTicketStore(cache);
        var ticket = CreateTicket(clock.GetUtcNow().AddMinutes(10));
        var key = await store.StoreAsync(ticket);

        clock.Advance(TimeSpan.FromMinutes(11));

        Assert.Null(await store.RetrieveAsync(key));
        Assert.False(cache.TryGetValue(key, out _));
    }

    [Fact]
    public async Task RequestsCannotChangeStoredClaimsOrTokensWithoutRenewing()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var store = new TestInMemoryTicketStore(cache);
        var ticket = CreateTicket(DateTimeOffset.UtcNow.AddMinutes(10));
        var key = await store.StoreAsync(ticket);
        var secondKey = await store.StoreAsync(ticket);
        Assert.NotEqual(key, secondKey);

        ticket.Properties.UpdateTokenValue("access_token", "changed-after-store");
        var retrieved = await store.RetrieveAsync(key);
        Assert.Equal(AccessToken, retrieved.Properties.GetTokenValue("access_token"));
        retrieved.Properties.UpdateTokenValue("access_token", "changed-in-request");
        ((ClaimsIdentity)retrieved.Principal.Identity!).AddClaim(new Claim("request-only", "value"));

        var unchanged = await store.RetrieveAsync(key);
        Assert.Equal(AccessToken, unchanged.Properties.GetTokenValue("access_token"));
        Assert.Null(unchanged.Principal.FindFirst("request-only"));

        await store.RenewAsync(key, retrieved);
        Assert.Equal("changed-in-request", (await store.RetrieveAsync(key)).Properties.GetTokenValue("access_token"));
        Assert.Equal(AccessToken, (await store.RetrieveAsync(secondKey)).Properties.GetTokenValue("access_token"));
        Assert.Null(await store.RetrieveAsync("unknown-session"));
    }

    private static AuthenticationTicket CreateTicket(DateTimeOffset expiresUtc, string scheme = CookieAuthenticationDefaults.AuthenticationScheme)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user"),
            new Claim("large_claim", LargeClaim)
        }, scheme));
        var properties = new AuthenticationProperties { ExpiresUtc = expiresUtc };
        properties.StoreTokens(new[]
        {
            new AuthenticationToken { Name = "access_token", Value = AccessToken },
            new AuthenticationToken { Name = "refresh_token", Value = RefreshToken },
            new AuthenticationToken { Name = "id_token", Value = "saved-id-token" }
        });
        return new AuthenticationTicket(principal, properties, scheme);
    }

    private static CookieAuthenticationOptions GetCookieOptions(TestServer server, bool saml = false) =>
        server.Services.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(saml ? Saml2Constants.AuthenticationScheme : CookieAuthenticationDefaults.AuthenticationScheme);

    private static async Task<IHost> CreateHost(TestClock clock, bool saml = false)
    {
        var scheme = saml ? Saml2Constants.AuthenticationScheme : CookieAuthenticationDefaults.AuthenticationScheme;
        return await new HostBuilder().ConfigureWebHost(web => web.UseTestServer()
            .ConfigureServices(services =>
            {
                services.AddDataProtection().UseEphemeralDataProtectionProvider();
                if (saml)
                {
                    services.AddSaml2("/Saml/Login", cookieSameSite: SameSiteMode.None);
                    services.AddTestInMemoryTicketStore(scheme);
                }
                else
                {
                    services.AddTestInMemoryTicketStore();
                    services.AddAuthentication(scheme).AddCookie();
                }
                services.Configure<MemoryCacheOptions>(options => options.Clock = clock);
                services.Configure<CookieAuthenticationOptions>(scheme, options =>
                {
                    options.TimeProvider = clock;
                    // Renew explicitly on /refresh so the expiration assertion is deterministic.
                    options.SlidingExpiration = false;
                    options.Events.OnValidatePrincipal = context =>
                    {
                        if (context.Request.Path == "/refresh")
                        {
                            context.Properties.UpdateTokenValue("access_token", "renewed-access-token");
                            context.Properties.UpdateTokenValue("refresh_token", "renewed-refresh-token");
                            context.ShouldRenew = true;
                        }
                        return Task.CompletedTask;
                    };
                });
            })
            .Configure(app =>
            {
                app.UseAuthentication();
                app.Run(async context =>
                {
                    if (context.Request.Path == "/login")
                    {
                        var ticket = CreateTicket(clock.GetUtcNow().AddMinutes(10), scheme);
                        await context.SignInAsync(scheme, ticket.Principal, ticket.Properties);
                    }
                    else if (context.Request.Path == "/logout")
                    {
                        await context.SignOutAsync(scheme);
                    }
                    else
                    {
                        var result = await context.AuthenticateAsync(scheme);
                        if (!result.Succeeded)
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return;
                        }
                        await context.Response.WriteAsync($"{result.Properties!.GetTokenValue("access_token")}|{result.Properties.GetTokenValue("refresh_token")}|{result.Principal!.FindFirst("large_claim")!.Value.Length}");
                    }
                });
            })).StartAsync();
    }

    private sealed class TestClock : TimeProvider, Microsoft.Extensions.Internal.ISystemClock
    {
        public DateTimeOffset UtcNow { get; private set; } = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => UtcNow;
        public void Advance(TimeSpan amount) => UtcNow += amount;
    }
}
