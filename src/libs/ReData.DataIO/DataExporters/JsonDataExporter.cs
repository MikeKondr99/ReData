using System.Data.Common;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ReData.DataIO.DataExporters;

public sealed class JsonDataExporter : IDataExporter 
{
    public async Task ExportAsync(DbDataReader reader, Stream outputStream, CancellationToken ct)
    {
        try
        {
            await using var writer = new Utf8JsonWriter(outputStream, new JsonWriterOptions
            {
                Indented = false,
                SkipValidation = false,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            });
            
            await WriteJsonArrayAsync(reader, writer, ct);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to export data to JSON", ex);
        }
    }
    
    private static async Task WriteJsonArrayAsync(DbDataReader reader, Utf8JsonWriter writer, CancellationToken ct)
    {
        writer.WriteStartArray();
        try
        {
            string[] propNames = new string[reader.FieldCount];
            for (int i = 0; i < propNames.Length; i++)
            {
                propNames[i] = reader.GetName(i);
            }
            
            while (await reader.ReadAsync(ct))
            {
                WriteRowAsObject(reader, writer, propNames);
            }
        }
        finally
        {
            writer.WriteEndArray();
            await writer.FlushAsync(ct);
        }
    }
    
    private static void WriteRowAsObject(DbDataReader reader, Utf8JsonWriter writer, string[] propNames)
    {
        writer.WriteStartObject();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            WriteJsonPropValue(propNames[i], reader.GetValue(i), writer);
        }
        writer.WriteEndObject();
    }

    private static void WriteJsonPropValue(string prop, object? value, Utf8JsonWriter writer)
    {
        switch (value)
        {
            case null:
                writer.WriteNull(prop);
                break;
            case DBNull:
                writer.WriteNull(prop);
                break;
            case string s:
                writer.WriteString(prop, s);
                break;
            case int i:
                writer.WriteNumber(prop, i);
                break;
            case long l:
                writer.WriteNumber(prop, l);
                break;
            case decimal d:
                writer.WriteNumber(prop, d);
                break;
            case double dbl:
                writer.WriteNumber(prop, dbl);
                break;
            case float f:
                writer.WriteNumber(prop, f);
                break;
            case bool b:
                writer.WriteBoolean(prop, b);
                break;
            case DateTime dt:
                writer.WriteString(prop, dt);
                break;
            case DateTimeOffset dto:
                writer.WriteString(prop, dto);
                break;
            case Guid g:
                writer.WriteString(prop, g);
                break;
            default:
                throw new NotSupportedException(
                    $" value of type '{value.GetType().Name}' not supported by json data exporter");
        }
    }
}