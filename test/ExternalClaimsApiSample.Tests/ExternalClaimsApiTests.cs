using ExternalClaimsApiSample.Models;
using FoxIDs.SampleHelperLibrary.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace ExternalClaimsApiSample.Tests;

public class ExternalClaimsApiTests
{
    [Fact]
    public void AppSettings_OmittedClaimsFormat_DefaultsToClaimsList() =>
        Assert.Equal(ClaimsFormats.ClaimsList, new AppSettings().ClaimsFormat);

    [Theory]
    [InlineData(ClaimsFormats.ClaimsList)]
    [InlineData(ClaimsFormats.Properties)]
    public async Task Claims_ConfiguredFormat_ReturnsSubjectAndAllRoles(ClaimsFormats format)
    {
        using var factory = new SampleApplicationFactory(format);
        using var client = factory.CreateApiClient();
        var request = format == ClaimsFormats.Properties
            ? """{"sub":"original","email":"USER@somewhere.org","role":["reader","writer"]}"""
            : """{"claims":[{"type":"sub","value":"original"},{"type":"email","value":"USER@somewhere.org"},{"type":"role","value":"reader"},{"type":"role","value":"writer"}]}""";
        var response = await PostAsync(client, request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadAsync(response);
        if (format == ClaimsFormats.Properties)
        {
            Assert.False(body.TryGetProperty("claims", out _));
            Assert.Equal("somewhere/external-USER@somewhere.org", body.GetProperty("sub").GetString());
            Assert.Equal(new[] { "admin_access", "read_access", "write_access" },
                body.GetProperty("role").EnumerateArray().Select(value => value.GetString()));
        }
        else
        {
            Assert.Single(body.EnumerateObject());
            var claims = body.GetProperty("claims").EnumerateArray().ToArray();
            Assert.Equal("somewhere/external-USER@somewhere.org", claims.Single(claim => claim.GetProperty("type").GetString() == "sub").GetProperty("value").GetString());
            Assert.Equal(new[] { "admin_access", "read_access", "write_access" },
                claims.Where(claim => claim.GetProperty("type").GetString() == "role").Select(claim => claim.GetProperty("value").GetString()));
        }
    }

    [Theory]
    [InlineData(ClaimsFormats.ClaimsList, """{"claims":[]}""")]
    [InlineData(ClaimsFormats.Properties, "{}")]
    [InlineData(ClaimsFormats.Properties, """{"role":[]}""")]
    public async Task Claims_EmptyClaims_UsesExistingUnknownSubjectBehaviour(ClaimsFormats format, string request)
    {
        using var factory = new SampleApplicationFactory(format);
        using var client = factory.CreateApiClient();
        var response = await PostAsync(client, request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("somewhere/external-unknown", await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData(ClaimsFormats.ClaimsList, """{"sub":"user"}""")]
    [InlineData(ClaimsFormats.ClaimsList, """{"claims":{"sub":"user"}}""")]
    [InlineData(ClaimsFormats.Properties, """{"claims":[{"type":"sub","value":"user"}]}""")]
    [InlineData(ClaimsFormats.Properties, """{"sub":123}""")]
    [InlineData(ClaimsFormats.Properties, """{"sub":true}""")]
    [InlineData(ClaimsFormats.Properties, """{"sub":null}""")]
    [InlineData(ClaimsFormats.Properties, """{"role":["reader",null]}""")]
    [InlineData(ClaimsFormats.Properties, """{"sub":{"value":"user"}}""")]
    [InlineData(ClaimsFormats.Properties, """{"sub":"first","sub":"second"}""")]
    [InlineData(ClaimsFormats.Properties, """{"":"user"}""")]
    [InlineData(ClaimsFormats.Properties, """{"sub":""}""")]
    [InlineData(ClaimsFormats.Properties, "[]")]
    [InlineData(ClaimsFormats.Properties, "null")]
    public async Task Claims_InvalidOrMismatchedFormat_ReturnsBadRequest(ClaimsFormats format, string request)
    {
        using var factory = new SampleApplicationFactory(format);
        using var client = factory.CreateApiClient();
        Assert.Equal(HttpStatusCode.BadRequest, (await PostAsync(client, request)).StatusCode);
    }

    [Theory]
    [InlineData(ClaimsFormats.ClaimsList, """{"claims":[{"type":"sub","value":"user"}]}""")]
    [InlineData(ClaimsFormats.Properties, """{"sub":"user"}""")]
    public async Task Claims_WrongSecret_ReturnsExistingAuthenticationError(ClaimsFormats format, string request)
    {
        using var factory = new SampleApplicationFactory(format);
        using var client = factory.CreateApiClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes("external_claims:wrong-secret")));
        var response = await PostAsync(client, request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("invalid_api_id_secret", (await ReadAsync(response)).GetProperty("error").GetString());
    }

    [Theory]
    [InlineData(ClaimsFormats.ClaimsList)]
    [InlineData(ClaimsFormats.Properties)]
    public async Task Swagger_ConfiguredFormat_DescribesRequestAndResponse(ClaimsFormats format)
    {
        using var factory = new SampleApplicationFactory(format);
        using var client = factory.CreateApiClient();
        var document = await ReadAsync(await client.GetAsync("/swagger/v1/swagger.json"));
        var operation = document.GetProperty("paths").GetProperty("/ExternalClaims/Claims").GetProperty("post");
        var request = Resolve(document, operation.GetProperty("requestBody").GetProperty("content").GetProperty("application/json").GetProperty("schema"));
        var response = Resolve(document, operation.GetProperty("responses").GetProperty("200").GetProperty("content").GetProperty("application/json").GetProperty("schema"));
        foreach (var schema in new[] { request, response })
        {
            Assert.Equal("object", schema.GetProperty("type").GetString());
            if (format == ClaimsFormats.Properties)
            {
                Assert.Equal(2, schema.GetProperty("additionalProperties").GetProperty("oneOf").GetArrayLength());
                Assert.False(schema.TryGetProperty("properties", out var properties) && properties.TryGetProperty("claims", out _));
            }
            else
            {
                Assert.Equal("array", schema.GetProperty("properties").GetProperty("claims").GetProperty("type").GetString());
            }
        }
    }

    private static JsonElement Resolve(JsonElement document, JsonElement schema) =>
        schema.TryGetProperty("$ref", out var reference)
            ? document.GetProperty("components").GetProperty("schemas").GetProperty(reference.GetString().Split('/').Last())
            : schema;

    private static Task<HttpResponseMessage> PostAsync(HttpClient client, string body) =>
        client.PostAsync("/ExternalClaims/Claims", new StringContent(body, Encoding.UTF8, "application/json"));

    private static Task<JsonElement> ReadAsync(HttpResponseMessage response) =>
        response.Content.ReadFromJsonAsync<JsonElement>();
}
