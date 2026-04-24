using System.Data.Common;
using System.Globalization;
using System.Text.Json;

namespace ReData.DemoApp.Endpoints.Transform;

/// <summary>
/// Обертка над ресурсами потоковой выдачи JSON-коллекции.
/// </summary>
public sealed class JsonCollection : IAsyncDisposable
{
    public JsonCollection(DbDataReader dataReader, DbConnection connection)
    {
        DataReader = dataReader;
        Connection = connection;
    }

    private DbDataReader DataReader { get; }

    private DbConnection Connection { get; }

    public async Task WriteToAsync(Utf8JsonWriter writer, CancellationToken ct, int flushEvery = 1000)
    {
        writer.WriteStartArray();

        var propNames = new string[DataReader.FieldCount];
        for (var i = 0; i < propNames.Length; i++)
        {
            propNames[i] = DataReader.GetName(i);
        }

        var written = 0;
        while (await DataReader.ReadAsync(ct))
        {
            writer.WriteStartObject();
            for (var i = 0; i < DataReader.FieldCount; i++)
            {
                WriteJsonPropValue(propNames[i], DataReader.GetValue(i), writer);
            }

            writer.WriteEndObject();
            written++;

            if (written % flushEvery == 0)
            {
                await writer.FlushAsync(ct);
            }
        }

        writer.WriteEndArray();
    }

    public async ValueTask DisposeAsync()
    {
        await DataReader.DisposeAsync();
        await Connection.DisposeAsync();
    }

    private static void WriteJsonPropValue(string prop, object? value, Utf8JsonWriter writer)
    {
        switch (value)
        {
            case null:
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
                writer.WriteString(prop, Convert.ToString(value, CultureInfo.InvariantCulture));
                break;
        }
    }
}
