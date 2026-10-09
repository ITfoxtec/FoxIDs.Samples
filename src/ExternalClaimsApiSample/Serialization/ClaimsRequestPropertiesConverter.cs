using ExternalClaimsApiSample.Models.Api;
using FoxIDs.SampleHelperLibrary.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExternalClaimsApiSample.Serialization;

public class ClaimsRequestPropertiesConverter : JsonConverter<ClaimsRequest>
{
    public override ClaimsRequest Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new() { Claims = ClaimProperties.Read(ref reader, (type, value) => new ClaimValue { Type = type, Value = value }) };

    public override void Write(Utf8JsonWriter writer, ClaimsRequest value, JsonSerializerOptions options) =>
        ClaimProperties.Write(writer, value.Claims, claim => claim.Type, claim => claim.Value);
}
