using System.Text.Json;
using System.Text.Json.Serialization;
using ReData.Query.Runners.Value;

namespace ReData.DemoApp.Converters;

public class ValueConverter : JsonConverter<IValue>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(IValue).IsAssignableFrom(typeToConvert);
    }
    
    public override IValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number when reader.TryGetInt32(out int intVal) 
                => new IntegerValue(intVal),
            JsonTokenType.Number 
                => new NumberValue(reader.GetDouble()),
            JsonTokenType.String when DateTime.TryParse(reader.GetString(), out var dateVal)
                => new DateTimeValue(dateVal),
            JsonTokenType.String
                => new TextValue(reader.GetString()!),
            JsonTokenType.True 
                => new BoolValue(true),
            JsonTokenType.False
                => new BoolValue(false),
            _ => throw new JsonException($"Unsupported JSON token type: {reader.TokenType}")
        };
    }

    public override void Write(Utf8JsonWriter writer, IValue value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case IntegerValue iv:
                writer.WriteNumberValue(iv.Value);
                break;
            case NumberValue nv:
                writer.WriteNumberValue(nv.Value);
                break;
            case TextValue tv:
                writer.WriteStringValue(tv.Value);
                break;
            case DateTimeValue dtv:
                writer.WriteStringValue(dtv.Value);
                break;
            case BoolValue bv:
                writer.WriteBooleanValue(bv.Value);
                break;
            case NullValue nv:
                writer.WriteNullValue();
                break;
            case UnknownValue uv:
                writer.WriteStartObject();
                writer.WriteBoolean("unknown", true);
                writer.WriteString("type", uv.Type);
                writer.WriteEndObject();
                break;
            default:
                throw new JsonException($"Unsupported IValue type: {value.GetType().Name}");
        }
    }
}