using System.Text.Json;
using System.Text.Json.Serialization;
using CoreLib.Models;

namespace CoreLib.Serialization;
public class IntegerIntervalJsonConverter : JsonConverter<IntegerInterval>
{
    public override IntegerInterval Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject token");

        int min = 0, max = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return new IntegerInterval(min, max);

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName?.ToLowerInvariant())
                {
                    case "min":
                        min = reader.GetInt32();
                        break;
                    case "max":
                        max = reader.GetInt32();
                        break;
                }
            }
        }

        throw new JsonException("Unexpected end of JSON");
    }

    public override void Write(Utf8JsonWriter writer, IntegerInterval value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("min", value.Min);
        writer.WriteNumber("max", value.Max);
        writer.WriteEndObject();
    }
}