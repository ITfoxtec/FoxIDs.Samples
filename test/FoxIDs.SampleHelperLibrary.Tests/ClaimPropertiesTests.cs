using FoxIDs.SampleHelperLibrary.Serialization;
using System.Text;
using System.Text.Json;
using Xunit;

namespace FoxIDs.SampleHelperLibrary.Tests;

public class ClaimPropertiesTests
{
    [Fact]
    public void Read_StringsAndArrays_PreservesCaseContentsAndRepeatedValues()
    {
        var claims = Read("""{"Name":" User ","name":"lower","role":["reader","reader"],"when":"2026-10-09T12:34:56Z","empty":[]}""");
        Assert.Equal(new[]
        {
            new KeyValuePair<string, string>("Name", " User "),
            new KeyValuePair<string, string>("name", "lower"),
            new KeyValuePair<string, string>("role", "reader"),
            new KeyValuePair<string, string>("role", "reader"),
            new KeyValuePair<string, string>("when", "2026-10-09T12:34:56Z")
        }, claims);
    }

    [Theory]
    [InlineData("[]")]
    [InlineData("null")]
    [InlineData("""{"role":1}""")]
    [InlineData("""{"role":true}""")]
    [InlineData("""{"role":null}""")]
    [InlineData("""{"role":{}}""")]
    [InlineData("""{"role":["reader",null]}""")]
    [InlineData("""{"role":[["reader"]]}""")]
    [InlineData("""{"role":"reader","role":"writer"}""")]
    public void Read_InvalidProperties_ThrowsJsonException(string json) =>
        Assert.Throws<JsonException>(() => Read(json));

    [Fact]
    public void Write_RepeatedTypes_WritesStringArraysWithoutChangingNamesOrValues()
    {
        var claims = Read("""{"Name":" User ","name":"lower","role":["reader","writer","reader"]}""");
        Assert.Equal("""{"Name":" User ","name":"lower","role":["reader","writer","reader"]}""", Write(claims));
    }

    [Fact]
    public void Write_EmptyClaims_WritesEmptyObject() =>
        Assert.Equal("{}", Write(Array.Empty<KeyValuePair<string, string>>()));

    private static List<KeyValuePair<string, string>> Read(string json)
    {
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();
        return ClaimProperties.Read(ref reader, (type, value) => new KeyValuePair<string, string>(type, value));
    }

    private static string Write(IEnumerable<KeyValuePair<string, string>> claims)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            ClaimProperties.Write(writer, claims, claim => claim.Key, claim => claim.Value);
        }
        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
