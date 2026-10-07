using DirectoryConnectorApiSample.Controllers;
using DirectoryConnectorApiSample.Models;
using DirectoryConnectorApiSample.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace DirectoryConnectorApiSample.Tests;

public class DirectoryConnectorApiTests : IDisposable
{
    private readonly IHost host;
    private readonly HttpClient client;

    public DirectoryConnectorApiTests()
    {
        host = new HostBuilder().ConfigureWebHost(webBuilder => webBuilder.UseTestServer()
            .ConfigureServices(services =>
            {
                services.AddSingleton(new AppSettings { ApiSecret = "test-secret" });
                services.AddSingleton<DemoDirectoryStore>();
                services.Configure<RequestLocalizationOptions>(options =>
                {
                    options.SetDefaultCulture("en").AddSupportedCultures("en", "da").AddSupportedUICultures("en", "da");
                    options.RequestCultureProviders = [new AcceptLanguageHeaderRequestCultureProvider()];
                });
                services.AddControllers().AddApplicationPart(typeof(DirectoryConnectorController).Assembly);
            })
            .Configure(app =>
            {
                app.UseRequestLocalization();
                app.UseRouting();
                app.UseEndpoints(endpoints => endpoints.MapControllers());
            })).Start();
        client = host.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes("directory_connector:test-secret")));
    }

    [Theory]
    [InlineData("email", " USER1@SOMEWHERE.ORG ")]
    [InlineData("phone", "+4511223344")]
    [InlineData("username", "USER1")]
    [InlineData("directoryUserId", "dir-user-1")]
    public async Task SynchroniseUser_KnownIdentifier_ReturnsFullSnapshot(string key, string value)
    {
        var response = await client.PostAsJsonAsync("/synchronise-user", new Dictionary<string, string> { [key] = value });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await JsonAsync(response);
        Assert.Equal("dir-user-1", user.GetProperty("directoryUserId").GetString());
        Assert.Equal("user1@somewhere.org", user.GetProperty("email").GetString());
        Assert.False(user.GetProperty("disableAccount").GetBoolean());
        Assert.False(user.GetProperty("setPasswordEmail").GetBoolean());
        Assert.True(user.TryGetProperty("passwordLastChanged", out _));
        Assert.Equal(2, user.GetProperty("claims").GetArrayLength());
        Assert.False(user.TryGetProperty("password", out _));
    }

    [Theory]
    [InlineData("dir-user-2", 200, null)]
    [InlineData("missing", 403, "user_deleted")]
    [InlineData("DIR-USER-1", 403, "user_deleted")]
    public async Task SynchroniseUser_DirectoryIdIsAuthoritative_DoesNotFallBackToEmail(string id, int status, string error)
    {
        var response = await client.PostAsJsonAsync("/synchronise-user", new { directoryUserId = id, email = "user1@somewhere.org" });
        Assert.Equal(status, (int)response.StatusCode);
        var body = await JsonAsync(response);
        Assert.Equal(error ?? id, body.GetProperty(error == null ? "directoryUserId" : "error").GetString());
    }

    [Theory]
    [InlineData("disabled@somewhere.org", 200, null)]
    [InlineData("deleted@somewhere.org", 401, "user_not_exists")]
    [InlineData("missing@somewhere.org", 401, "user_not_exists")]
    public async Task SynchroniseUser_AccountState_ReturnsSnapshotOrLookupError(string email, int status, string error)
    {
        var response = await client.PostAsJsonAsync("/synchronise-user", new { email });
        Assert.Equal(status, (int)response.StatusCode);
        var body = await JsonAsync(response);
        if (error == null) Assert.True(body.GetProperty("disableAccount").GetBoolean());
        else Assert.Equal(error, body.GetProperty("error").GetString());
    }

    [Theory]
    [InlineData("authentication", "dir-user-disabled")]
    [InlineData("authentication", "dir-user-deleted")]
    [InlineData("authentication", "missing")]
    [InlineData("change-password", "dir-user-disabled")]
    [InlineData("change-password", "dir-user-deleted")]
    [InlineData("change-password", "missing")]
    [InlineData("set-password", "dir-user-disabled")]
    [InlineData("set-password", "dir-user-deleted")]
    [InlineData("set-password", "missing")]
    public async Task PasswordAction_UnavailableAccount_ReturnsOperationRejectedWithoutUiMessage(string endpoint, string id)
    {
        var response = await client.PostAsJsonAsync(endpoint, PasswordRequest(id));
        await AssertErrorAsync(response, HttpStatusCode.Forbidden, "operation_rejected");
    }

    [Theory]
    [InlineData("authentication")]
    [InlineData("change-password")]
    [InlineData("set-password")]
    public async Task PasswordAction_ValidRequest_ReturnsNoContent(string endpoint)
    {
        var response = await client.PostAsJsonAsync(endpoint, PasswordRequest("dir-user-1"));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("authentication")]
    [InlineData("change-password")]
    [InlineData("set-password")]
    public async Task PasswordAction_IdentifierWithoutDirectoryId_IsRejected(string endpoint)
    {
        var response = await client.PostAsJsonAsync(endpoint, new { email = "user1@somewhere.org", password = "testpass1", currentPassword = "testpass1", newPassword = "newpass123" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("testpass1", host.Services.GetRequiredService<DemoDirectoryStore>().Find("dir-user-1").Password);
    }

    [Theory]
    [InlineData("synchronise-user")]
    [InlineData("authentication")]
    [InlineData("create-user")]
    [InlineData("change-password")]
    [InlineData("set-password")]
    public async Task Endpoint_InvalidApiCredentials_ReturnsOnlyApiAuthenticationError(string endpoint)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("directory_connector:wrong-secret")));
        var response = await client.PostAsJsonAsync(endpoint, PasswordRequest("dir-user-1"));
        await AssertErrorAsync(response, HttpStatusCode.Unauthorized, "invalid_api_id_secret");
    }

    [Theory]
    [InlineData("en", "dir-user-rejected", "testpass3", "login_rejected", "You cannot log in here. Contact support.")]
    [InlineData("da-DK", "dir-user-rejected", "testpass3", "login_rejected", "Du kan ikke logge ind her. Kontakt support.")]
    [InlineData("fr-FR", "dir-user-rejected", "testpass3", "login_rejected", "You cannot log in here. Contact support.")]
    [InlineData("en", "dir-user-rejected-no-message", "testpass4", "login_rejected", null)]
    [InlineData("en", "dir-user-rejected", "short", "invalid_password", null)]
    [InlineData("en", "dir-user-expired", "short", "invalid_password", null)]
    public async Task Authenticate_Rejection_VerifiesCredentialsBeforeAccountGuidance(string language, string id, string password, string error, string message)
    {
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd(language);
        var response = await client.PostAsJsonAsync("authentication", new { directoryUserId = id, password });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await JsonAsync(response);
        Assert.Equal(error, body.GetProperty("error").GetString());
        if (message == null) Assert.False(body.TryGetProperty("uiErrorMessage", out _));
        else Assert.Equal(message, body.GetProperty("uiErrorMessage").GetString());
        Assert.Equal(password == "short" ? "Invalid password." : "Credentials verified; login rejected by demo directory policy.", body.GetProperty("errorMessage").GetString());
    }

    [Fact]
    public async Task ChangePassword_ExpiredCredential_CanChangeAfterVerification()
    {
        var expired = await client.PostAsJsonAsync("authentication", new { directoryUserId = "dir-user-expired", password = "testpass6" });
        await AssertErrorAsync(expired, HttpStatusCode.BadRequest, "password_expired");
        var wrong = await client.PostAsJsonAsync("change-password", new { directoryUserId = "dir-user-expired", currentPassword = "wrong", newPassword = "short" });
        await AssertErrorAsync(wrong, HttpStatusCode.Unauthorized, "invalid_current_password");
        var changed = await client.PostAsJsonAsync("change-password", new { directoryUserId = "dir-user-expired", currentPassword = "testpass6", newPassword = "newpass123" });
        Assert.Equal(HttpStatusCode.NoContent, changed.StatusCode);
        var authenticated = await client.PostAsJsonAsync("authentication", new { directoryUserId = "dir-user-expired", password = "newpass123" });
        Assert.Equal(HttpStatusCode.NoContent, authenticated.StatusCode);
        var sync = await client.PostAsJsonAsync("synchronise-user", new { directoryUserId = "dir-user-expired" });
        var user = await JsonAsync(sync);
        Assert.False(user.GetProperty("changePassword").GetBoolean());
        Assert.True(user.GetProperty("passwordLastChanged").GetInt64() > 0);
    }

    [Theory]
    [InlineData("dir-user-setup-email", "setPasswordEmail")]
    [InlineData("dir-user-setup-sms", "setPasswordSms")]
    public async Task SetPassword_FirstSignIn_ClearsSetupRequirementAndSetsTimestamp(string id, string flag)
    {
        var sync = await client.PostAsJsonAsync("synchronise-user", new { directoryUserId = id });
        Assert.True((await JsonAsync(sync)).GetProperty(flag).GetBoolean());
        var result = await client.PostAsJsonAsync("set-password", new { directoryUserId = id, password = "newpass123" });
        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        var snapshot = await JsonAsync(await client.PostAsJsonAsync("synchronise-user", new { directoryUserId = id }));
        Assert.False(snapshot.GetProperty(flag).GetBoolean());
        Assert.InRange(snapshot.GetProperty("passwordLastChanged").GetInt64(), DateTimeOffset.UtcNow.AddMinutes(-1).ToUnixTimeSeconds(), DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsJsonAsync("authentication", new { directoryUserId = id, password = "newpass123" })).StatusCode);
    }

    [Fact]
    public async Task CreateUser_NewAccount_ReturnsOnlyIdThenCanSynchroniseAndAuthenticate()
    {
        var response = await client.PostAsJsonAsync("create-user", new { email = "new@example.org", password = "newpass123", confirmAccount = true, requireMultiFactor = true });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var created = await JsonAsync(response);
        Assert.Single(created.EnumerateObject());
        var id = created.GetProperty("directoryUserId").GetString();
        var sync = await JsonAsync(await client.PostAsJsonAsync("synchronise-user", new { directoryUserId = id }));
        Assert.Equal("new@example.org", sync.GetProperty("email").GetString());
        Assert.True(sync.GetProperty("confirmAccount").GetBoolean());
        Assert.True(sync.GetProperty("requireMultiFactor").GetBoolean());
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsJsonAsync("authentication", new { directoryUserId = id, password = "newpass123" })).StatusCode);
        var duplicate = await client.PostAsJsonAsync("create-user", new { email = "NEW@example.org", password = "newpass123" });
        await AssertErrorAsync(duplicate, HttpStatusCode.BadRequest, "user_exists");
        await AssertErrorAsync(await client.PostAsJsonAsync("synchronise-user", new { username = "unknown" }), HttpStatusCode.Unauthorized, "user_not_exists");
    }

    [Theory]
    [InlineData("synchronise-user", false)]
    [InlineData("synchronise-user", true)]
    [InlineData("create-user", false)]
    [InlineData("create-user", true)]
    public async Task IdentifierRequest_ZeroOrMultipleIdentifiers_IsRejected(string endpoint, bool multiple)
    {
        var response = await client.PostAsJsonAsync(endpoint, new { email = multiple ? "new@example.org" : null, username = multiple ? "newuser" : null, password = "newpass123" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task NewPassword_SameAsCurrent_IsOnlyRejectedByChangePassword()
    {
        var changed = await client.PostAsJsonAsync("change-password", new { directoryUserId = "dir-user-1", currentPassword = "testpass1", newPassword = "testpass1" });
        await AssertErrorAsync(changed, HttpStatusCode.BadRequest, "new_password_equals_current");
        var reset = await client.PostAsJsonAsync("set-password", new { directoryUserId = "dir-user-1", password = "testpass1" });
        Assert.Equal(HttpStatusCode.NoContent, reset.StatusCode);
    }

    private static object PasswordRequest(string id) => new { directoryUserId = id, email = "new@example.org", password = "testpass1", currentPassword = "testpass1", newPassword = "newpass123" };

    private static async Task<JsonElement> JsonAsync(HttpResponseMessage response) =>
        JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());

    private static async Task AssertErrorAsync(HttpResponseMessage response, HttpStatusCode status, string error)
    {
        Assert.Equal(status, response.StatusCode);
        var body = await JsonAsync(response);
        Assert.Equal(error, body.GetProperty("error").GetString());
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("errorMessage").GetString()));
        Assert.False(body.TryGetProperty("uiErrorMessage", out _));
        Assert.False(body.TryGetProperty("directoryUserId", out _));
    }

    public void Dispose()
    {
        client.Dispose();
        host.Dispose();
    }
}
