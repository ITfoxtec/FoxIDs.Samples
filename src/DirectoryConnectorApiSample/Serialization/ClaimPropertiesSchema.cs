using Microsoft.OpenApi;
using System.Text.Json.Nodes;

namespace DirectoryConnectorApiSample.Serialization;

public static class ClaimPropertiesSchema
{
    public static OpenApiSchema Create() => new()
    {
        Type = JsonSchemaType.Object,
        Description = "Claim names with a string value or an array of strings for multiple values.",
        AdditionalProperties = new OpenApiSchema
        {
            OneOf =
            [
                new OpenApiSchema { Type = JsonSchemaType.String },
                new OpenApiSchema { Type = JsonSchemaType.Array, Items = new OpenApiSchema { Type = JsonSchemaType.String } }
            ]
        },
        Example = JsonNode.Parse("""{"name":"User Two","role":["admin_access","read_access","write_access"]}""")
    };
}
