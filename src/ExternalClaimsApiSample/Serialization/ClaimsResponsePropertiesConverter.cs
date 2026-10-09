using ExternalClaimsApiSample.Models.Api;
using FoxIDs.SampleHelperLibrary.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExternalClaimsApiSample.Serialization;

public class ClaimsResponsePropertiesConverter : JsonConverter<ClaimsResponse>
{
    public override ClaimsResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new() { Claims = ClaimProperties.Read(ref reader, (type, value) => new ClaimValue { Type = type, Value = value }) };

    public override void Write(Utf8JsonWriter writer, ClaimsResponse value, JsonSerializerOptions options) =>
        ClaimProperties.Write(writer, value.Claims, claim => claim.Type, claim => claim.Value);
}
