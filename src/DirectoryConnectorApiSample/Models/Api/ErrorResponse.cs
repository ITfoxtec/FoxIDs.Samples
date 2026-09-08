using System.Text.Json.Serialization;

namespace DirectoryConnectorApiSample.Models.Api;

public class ErrorResponse
{
    public string Error { get; set; }

    public string ErrorMessage { get; set; }

    // Only login_rejected may include user-facing text; ErrorMessage is diagnostic text for the logs.
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string UiErrorMessage { get; set; }
}
