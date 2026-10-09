using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace ExternalClaimsApiSample.Serialization;

public static class ClaimPropertiesSchema
{
    public static OpenApiSchema Create() => new()
    {
        Type = "object",
        Description = "Claim names at the JSON root, with a string value or an array of strings for multiple values.",
        AdditionalProperties = new OpenApiSchema
        {
            OneOf =
            [
                new OpenApiSchema { Type = "string" },
                new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Type = "string" } }
            ]
        },
        Example = new OpenApiObject
        {
            ["sub"] = new OpenApiString("somewhere/user1"),
            ["email"] = new OpenApiString("user1@somewhere.org"),
            ["role"] = new OpenApiArray { new OpenApiString("read_access"), new OpenApiString("write_access") }
        }
    };
}
