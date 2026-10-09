using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace FoxIDs.SampleHelperLibrary.Serialization;

public static class ClaimProperties
{
    public static List<TClaim> Read<TClaim>(ref Utf8JsonReader reader, Func<string, string, TClaim> createClaim)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Claims properties must be a JSON object.");
        }

        var claims = new List<TClaim>();
        var names = new HashSet<string>(StringComparer.Ordinal);
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected a claim property name.");
            }
            var type = reader.GetString();
            if (!names.Add(type))
            {
                throw new JsonException("Duplicate claim property names are not supported.");
            }

            reader.Read();
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    claims.Add(createClaim(type, ReadValue(ref reader)));
                }
            }
            else
            {
                claims.Add(createClaim(type, ReadValue(ref reader)));
            }
        }
        return claims;
    }

    public static void Write<TClaim>(Utf8JsonWriter writer, IEnumerable<TClaim> claims,
        Func<TClaim, string> getType, Func<TClaim, string> getValue)
    {
        writer.WriteStartObject();
        if (claims != null)
        {
            foreach (var group in claims.GroupBy(getType, StringComparer.Ordinal))
            {
                writer.WritePropertyName(group.Key);
                var values = group.Select(getValue).ToArray();
                if (values.Length == 1)
                {
                    writer.WriteStringValue(values[0]);
                }
                else
                {
                    writer.WriteStartArray();
                    foreach (var value in values)
                    {
                        writer.WriteStringValue(value);
                    }
                    writer.WriteEndArray();
                }
            }
        }
        writer.WriteEndObject();
    }

    private static string ReadValue(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Claim property values must be strings or arrays of strings.");
        }
        return reader.GetString();
    }
}
