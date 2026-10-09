using DirectoryConnectorApiSample.Models.Api;
using FoxIDs.SampleHelperLibrary.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DirectoryConnectorApiSample.Serialization;

public class ClaimPropertiesConverter : JsonConverter<IEnumerable<ClaimValue>>
{
    public override IEnumerable<ClaimValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        ClaimProperties.Read(ref reader, (type, value) => new ClaimValue { Type = type, Value = value });

    public override void Write(Utf8JsonWriter writer, IEnumerable<ClaimValue> value, JsonSerializerOptions options) =>
        ClaimProperties.Write(writer, value, claim => claim.Type, claim => claim.Value);
}
