using System.Text.Json;
using System.Text.Json.Serialization;
using ReData.Query.Core.Types;

namespace ReData.DemoApplication.Converters;

public class DataTypeJsonConverter : JsonConverter<DataType>
{
    public override DataType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            return stringValue?.ToLowerInvariant() switch
            {
                "unk" => DataType.Unknown,
                "null" => DataType.Null,
                "num" => DataType.Number,
                "int" => DataType.Integer,
                "text" => DataType.Text,
                "bool" => DataType.Bool,
                "date" => DataType.DateTime,
                _ => throw new JsonException($"Invalid DataType value: {stringValue}")
            };
        }
        
        if (reader.TokenType == JsonTokenType.Number)
        {
            // Also handle numeric values for backward compatibility
            var intValue = reader.GetInt32();
            if (Enum.IsDefined(typeof(DataType), intValue))
            {
                return (DataType)intValue;
            }
            throw new JsonException($"Invalid DataType numeric value: {intValue}");
        }

        throw new JsonException($"Unexpected token type: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, DataType value, JsonSerializerOptions options)
    {
        var stringValue = value switch
        {
            DataType.Unknown => "unk",
            DataType.Null => "null",
            DataType.Number => "num",
            DataType.Integer => "int",
            DataType.Text => "text",
            DataType.Bool => "bool",
            DataType.DateTime => "date",
            _ => throw new JsonException($"Unknown DataType value: {value}")
        };
        
        writer.WriteStringValue(stringValue);
    }
}