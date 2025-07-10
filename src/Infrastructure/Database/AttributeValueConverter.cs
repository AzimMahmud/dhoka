using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.Model;

namespace Infrastructure.Database;

public class AttributeValueConverter : JsonConverter<AttributeValue>
{
    public override AttributeValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonElement doc = JsonDocument.ParseValue(ref reader).RootElement;

        if (doc.TryGetProperty("S", out JsonElement s) && s.ValueKind != JsonValueKind.Null)
        {
            return new AttributeValue { S = s.GetString() };
        }

        if (doc.TryGetProperty("N", out JsonElement n) && n.ValueKind != JsonValueKind.Null)
        {
            return new AttributeValue { N = n.GetString() };
        }

        if (doc.TryGetProperty("BOOL", out JsonElement b) && b.ValueKind != JsonValueKind.Null)
        {
            return new AttributeValue { BOOL = b.GetBoolean() };
        }

        if (doc.TryGetProperty("NULL", out JsonElement nullValue) && nullValue.ValueKind != JsonValueKind.Null)
        {
            if (nullValue.GetBoolean())
            {
                return new AttributeValue { NULL = true };
            }
        }

        if (doc.TryGetProperty("L", out JsonElement list) && list.ValueKind != JsonValueKind.Null)
        {
            return new AttributeValue
            {
                L = list.EnumerateArray().Select(elem =>
                {
                    string json = elem.GetRawText();
                    return JsonSerializer.Deserialize<AttributeValue>(json, options);
                }).ToList()
            };
        }

        throw new JsonException("Unsupported or invalid AttributeValue type in JSON.");
    }

    public override void Write(Utf8JsonWriter writer, AttributeValue value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        if (value.NULL)
        {
            writer.WriteBoolean("NULL", true);
        }
        else if (value.S != null)
        {
            writer.WriteString("S", value.S);
        }
        else if (value.N != null)
        {
            writer.WriteString("N", value.N);
        }
        else if (value.BOOL)
        {
            writer.WriteBoolean("BOOL", value.BOOL);
        }
        else if (value.L != null)
        {
            writer.WritePropertyName("L");
            writer.WriteStartArray();
            foreach (AttributeValue? item in value.L)
            {
                JsonSerializer.Serialize(writer, item, options);
            }
            writer.WriteEndArray();
        }

        writer.WriteEndObject();
    }
}
