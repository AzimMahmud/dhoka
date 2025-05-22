using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.Model;

namespace Infrastructure.Posts;

public class AttributeValueConverter : JsonConverter<AttributeValue>
{
    public override AttributeValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonElement doc = JsonDocument.ParseValue(ref reader).RootElement;

        if (doc.TryGetProperty("S", out JsonElement s))
        {
            return new AttributeValue { S = s.GetString() };
        }

        if (doc.TryGetProperty("N", out JsonElement n))
        {
            return new AttributeValue { N = n.GetString() };
        }

        if (doc.TryGetProperty("BOOL", out JsonElement b))
        {
            return new AttributeValue { BOOL = b.GetBoolean() };
        }

        if (doc.TryGetProperty("L", out JsonElement list))
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

        throw new JsonException("Unsupported AttributeValue type.");
    }

    public override void Write(Utf8JsonWriter writer, AttributeValue value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        if (value.S != null)
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