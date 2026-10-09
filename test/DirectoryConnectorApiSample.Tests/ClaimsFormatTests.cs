using DirectoryConnectorApiSample.Models;
using DirectoryConnectorApiSample.Models.Api;
using DirectoryConnectorApiSample.Services;
using FoxIDs.SampleHelperLibrary.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace DirectoryConnectorApiSample.Tests;

public class ClaimsFormatTests
{
    [Fact]
    public void AppSettings_OmittedClaimsFormat_DefaultsToClaimsList() =>
        Assert.Equal(ClaimsFormats.ClaimsList, new AppSettings().ClaimsFormat);

    [Theory]
    [InlineData(ClaimsFormats.ClaimsList)]
    [InlineData(ClaimsFormats.Properties)]
    public async Task CreateUser_ConfiguredFormat_PreservesClaimsThroughSynchronisation(ClaimsFormats format)
    {
        using var factory = new SampleApplicationFactory(format);
        using var client = factory.CreateApiClient();
        var claims = format == ClaimsFormats.Properties
            ? """{"Name":" New User ","name":"lowercase","role":["reader","writer","reader"],"when":"2026-10-09T12:34:56Z"}"""
            : """[{"type":"Name","value":" New User "},{"type":"name","value":"lowercase"},{"type":"role","value":"reader"},{"type":"role","value":"writer"},{"type":"role","value":"reader"},{"type":"when","value":"2026-10-09T12:34:56Z"}]""";
        var create = await client.PostAsync("/create-user", new StringContent(
            """{"email":"new@example.org","password":"newpass123","confirmAccount":true,"requireMultiFactor":true,"claims":""" + claims + "}", Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Single(created.EnumerateObject());
        var id = created.GetProperty("directoryUserId").GetString();
        var response = await client.PostAsJsonAsync("/synchronise-user", new { directoryUserId = id });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("new@example.org", user.GetProperty("email").GetString());
        Assert.True(user.GetProperty("confirmAccount").GetBoolean());
        Assert.True(user.GetProperty("requireMultiFactor").GetBoolean());
        Assert.False(user.GetProperty("disableAccount").GetBoolean());
        Assert.False(user.TryGetProperty("password", out _));
        var returned = user.GetProperty("claims");
        if (format == ClaimsFormats.Properties)
        {
            Assert.Equal(" New User ", returned.GetProperty("Name").GetString());
            Assert.Equal("lowercase", returned.GetProperty("name").GetString());
            Assert.Equal(new[] { "reader", "writer", "reader" }, returned.GetProperty("role").EnumerateArray().Select(value => value.GetString()));
            Assert.Equal("2026-10-09T12:34:56Z", returned.GetProperty("when").GetString());
        }
        else
        {
            Assert.Equal(JsonSerializer.Serialize(JsonSerializer.Deserialize<JsonElement>(claims)), JsonSerializer.Serialize(returned));
        }
        var stored = factory.Services.GetRequiredService<DemoDirectoryStore>().Find(id);
        Assert.Equal(6, stored.Claims.Count);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsJsonAsync("/authentication", new { directoryUserId = id, password = "newpass123" })).StatusCode);
    }

    [Theory]
    [InlineData(ClaimsFormats.ClaimsList)]
    [InlineData(ClaimsFormats.Properties)]
    public async Task SynchroniseUser_DemoUserWithMultipleRoles_ReturnsConfiguredFormat(ClaimsFormats format)
    {
        using var factory = new SampleApplicationFactory(format);
        using var client = factory.CreateApiClient();
        var response = await client.PostAsJsonAsync("/synchronise-user", new { directoryUserId = "dir-user-2" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<JsonElement>();
        var claims = user.GetProperty("claims");
        var roles = format == ClaimsFormats.Properties
            ? claims.GetProperty("role").EnumerateArray().Select(value => value.GetString())
            : claims.EnumerateArray().Where(value => value.GetProperty("type").GetString() == "role").Select(value => value.GetProperty("value").GetString());
        Assert.Equal(new[] { "admin_access", "read_access", "write_access" }, roles);
    }

    [Theory]
    [InlineData(ClaimsFormats.ClaimsList, "")]
    [InlineData(ClaimsFormats.ClaimsList, ",\"claims\":null")]
    [InlineData(ClaimsFormats.ClaimsList, ",\"claims\":[]")]
    [InlineData(ClaimsFormats.Properties, "")]
    [InlineData(ClaimsFormats.Properties, ",\"claims\":null")]
    [InlineData(ClaimsFormats.Properties, ",\"claims\":{}")]
    [InlineData(ClaimsFormats.Properties, ",\"claims\":{\"role\":[]}")]
    public async Task CreateUser_EmptyClaims_ReturnsEmptyConfiguredFormat(ClaimsFormats format, string member)
    {
        using var factory = new SampleApplicationFactory(format);
        using var client = factory.CreateApiClient();
        var create = await client.PostAsync("/create-user", new StringContent(
            """{"username":"newuser","password":"testpass123" """ + member + "}", Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, create.StatusCode);
        var id = (await create.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("directoryUserId").GetString();
        var response = await client.PostAsJsonAsync("/synchronise-user", new { directoryUserId = id });
        var claims = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("claims");
        Assert.Equal(format == ClaimsFormats.Properties ? "{}" : "[]", claims.GetRawText());
    }

    [Theory]
    [InlineData(ClaimsFormats.ClaimsList, """{"role":"reader"}""")]
    [InlineData(ClaimsFormats.Properties, """[{"type":"role","value":"reader"}]""")]
    [InlineData(ClaimsFormats.Properties, """{"role":false}""")]
    [InlineData(ClaimsFormats.Properties, """{"role":1}""")]
    [InlineData(ClaimsFormats.Properties, """{"role":null}""")]
    [InlineData(ClaimsFormats.Properties, """{"role":["reader",1]}""")]
    [InlineData(ClaimsFormats.Properties, """{"role":{"value":"reader"}}""")]
    [InlineData(ClaimsFormats.Properties, """{"role":"reader","role":"writer"}""")]
    [InlineData(ClaimsFormats.Properties, """{"":"reader"}""")]
    [InlineData(ClaimsFormats.Properties, """{"role":""}""")]
    public async Task CreateUser_InvalidOrMismatchedClaims_DoesNotCreateUser(ClaimsFormats format, string claims)
    {
        using var factory = new SampleApplicationFactory(format);
        using var client = factory.CreateApiClient();
        var response = await client.PostAsync("/create-user", new StringContent(
            """{"email":"new@example.org","password":"newpass123","claims":""" + claims + "}", Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(factory.Services.GetRequiredService<DemoDirectoryStore>().Find(new DirectorySynchronisationRequest { Email = "new@example.org" }));
    }

    [Theory]
    [InlineData(ClaimsFormats.ClaimsList)]
    [InlineData(ClaimsFormats.Properties)]
    public async Task Swagger_ConfiguredFormat_DescribesNestedClaims(ClaimsFormats format)
    {
        using var factory = new SampleApplicationFactory(format);
        using var client = factory.CreateApiClient();
        var document = await client.GetFromJsonAsync<JsonElement>("/swagger/v1/swagger.json");
        var schemas = document.GetProperty("components").GetProperty("schemas");
        foreach (var type in new[] { "DirectoryCreateUserRequest", "DirectoryUserResponse" })
        {
            var properties = schemas.GetProperty(type).GetProperty("properties");
            var claims = properties.GetProperty("claims");
            Assert.True(properties.TryGetProperty("email", out _));
            Assert.Equal(format == ClaimsFormats.Properties ? "object" : "array", claims.GetProperty("type").GetString());
            if (format == ClaimsFormats.Properties)
            {
                Assert.Equal(2, claims.GetProperty("additionalProperties").GetProperty("oneOf").GetArrayLength());
            }
        }
    }
}
