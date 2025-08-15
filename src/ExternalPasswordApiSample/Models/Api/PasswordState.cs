using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace ExternalPasswordApiSample.Models.Api
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PasswordState
    {
        [EnumMember(Value = "current")]
        Current,
        [EnumMember(Value = "new")]
        New
    }
}
