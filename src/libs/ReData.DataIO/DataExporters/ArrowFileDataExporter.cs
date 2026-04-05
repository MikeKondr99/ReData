using System.Data.Common;
using System.Globalization;
using Apache.Arrow;
using Apache.Arrow.Ipc;

namespace ReData.DataIO.DataExporters;

public sealed class ArrowFileDataExporter : IDataExporter
{
    public async Task ExportAsync(DbDataReader reader, Stream outputStream, CancellationToken ct)
    {
        try
        {
            var (columnNames, rows) = await ReadRowsAsync(reader, ct);
            var columnKinds = InferColumnKinds(columnNames.Length, rows);
            var columnBuilders = CreateColumnBuilders(columnKinds);
            AppendRows(columnBuilders, rows);

            var batchBuilder = new RecordBatch.Builder();
            for (var i = 0; i < columnNames.Length; i++)
            {
                batchBuilder.Append(columnNames[i], true, columnBuilders[i].Build());
            }

            using var recordBatch = batchBuilder.Build();

            // ArrowFileWriter requires a seekable stream. ASP.NET response stream is not guaranteed
            // to be seekable, so write to a buffer first and then copy to the response stream.
            await using var buffer = new MemoryStream();
            using (var writer = new ArrowFileWriter(buffer, recordBatch.Schema, leaveOpen: true))
            {
                await writer.WriteStartAsync(ct);
                await writer.WriteRecordBatchAsync(recordBatch, ct);
                await writer.WriteEndAsync(ct);
            }

            buffer.Position = 0;
            await buffer.CopyToAsync(outputStream, ct);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to export data to Arrow file", ex);
        }
    }

    private static async Task<(string[] ColumnNames, List<object?[]> Rows)> ReadRowsAsync(
        DbDataReader reader,
        CancellationToken ct)
    {
        var fieldCount = reader.FieldCount;
        var columnNames = new string[fieldCount];
        for (var i = 0; i < fieldCount; i++)
        {
            columnNames[i] = reader.GetName(i);
        }

        var rows = new List<object?[]>();
        while (await reader.ReadAsync(ct))
        {
            var row = new object?[fieldCount];
            for (var i = 0; i < fieldCount; i++)
            {
                var value = reader.GetValue(i);
                row[i] = value is DBNull ? null : value;
            }

            rows.Add(row);
        }

        return (columnNames, rows);
    }

    private static ArrowColumnKind[] InferColumnKinds(int fieldCount, List<object?[]> rows)
    {
        var kinds = new ArrowColumnKind[fieldCount];
        System.Array.Fill(kinds, ArrowColumnKind.Null);

        foreach (var row in rows)
        {
            for (var i = 0; i < fieldCount; i++)
            {
                kinds[i] = Promote(kinds[i], ClassifyValue(row[i]));
            }
        }

        return kinds;
    }

    private static IArrowColumnBuilder[] CreateColumnBuilders(ArrowColumnKind[] kinds)
    {
        var result = new IArrowColumnBuilder[kinds.Length];
        for (var i = 0; i < kinds.Length; i++)
        {
            result[i] = kinds[i] switch
            {
                ArrowColumnKind.Int64 => new Int64ColumnBuilder(),
                ArrowColumnKind.Double => new DoubleColumnBuilder(),
                ArrowColumnKind.Boolean => new BooleanColumnBuilder(),
                ArrowColumnKind.Date64 => new Date64ColumnBuilder(),
                ArrowColumnKind.Null => new NullColumnBuilder(),
                _ => new StringColumnBuilder(),
            };
        }

        return result;
    }

    private static void AppendRows(IArrowColumnBuilder[] builders, List<object?[]> rows)
    {
        foreach (var row in rows)
        {
            for (var i = 0; i < builders.Length; i++)
            {
                builders[i].Append(row[i]);
            }
        }
    }

    private static ArrowColumnKind ClassifyValue(object? value) => value switch
    {
        null => ArrowColumnKind.Null,
        sbyte or byte or short or ushort or int or uint or long or ulong => ArrowColumnKind.Int64,
        float or double or decimal => ArrowColumnKind.Double,
        bool => ArrowColumnKind.Boolean,
        DateTime or DateTimeOffset => ArrowColumnKind.Date64,
        string or char or Guid or TimeSpan or byte[] => ArrowColumnKind.String,
        _ => ArrowColumnKind.String,
    };

    private static ArrowColumnKind Promote(ArrowColumnKind current, ArrowColumnKind next)
    {
        if (next == ArrowColumnKind.Null)
        {
            return current;
        }

        if (current == ArrowColumnKind.Null)
        {
            return next;
        }

        if (current == next)
        {
            return current;
        }

        if ((current == ArrowColumnKind.Int64 && next == ArrowColumnKind.Double) ||
            (current == ArrowColumnKind.Double && next == ArrowColumnKind.Int64))
        {
            return ArrowColumnKind.Double;
        }

        return ArrowColumnKind.String;
    }

    private static long ConvertToInt64(object value) => value switch
    {
        sbyte v => v,
        byte v => v,
        short v => v,
        ushort v => v,
        int v => v,
        uint v => v,
        long v => v,
        ulong v => checked((long)v),
        _ => throw new InvalidCastException($"Cannot convert '{value.GetType().FullName}' to Int64."),
    };

    private static double ConvertToDouble(object value) => value switch
    {
        sbyte v => v,
        byte v => v,
        short v => v,
        ushort v => v,
        int v => v,
        uint v => v,
        long v => v,
        ulong v => v,
        float v => v,
        double v => v,
        decimal v => (double)v,
        _ => throw new InvalidCastException($"Cannot convert '{value.GetType().FullName}' to Double."),
    };

    private static string ConvertToInvariantString(object value) => value switch
    {
        string s => s,
        char c => c.ToString(),
        DateTime dt => dt.ToString("O", CultureInfo.InvariantCulture),
        DateTimeOffset dto => dto.ToString("O", CultureInfo.InvariantCulture),
        TimeSpan ts => ts.ToString("c", CultureInfo.InvariantCulture),
        Guid g => g.ToString(),
        byte[] bytes => Convert.ToBase64String(bytes),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty,
        _ => value.ToString() ?? string.Empty,
    };

    private interface IArrowColumnBuilder
    {
        void Append(object? value);
        IArrowArray Build();
    }

    private sealed class Int64ColumnBuilder : IArrowColumnBuilder
    {
        private readonly Int64Array.Builder _builder = new();

        public void Append(object? value)
        {
            if (value is null)
            {
                _builder.AppendNull();
                return;
            }

            _builder.Append(ConvertToInt64(value));
        }

        public IArrowArray Build() => _builder.Build();
    }

    private sealed class DoubleColumnBuilder : IArrowColumnBuilder
    {
        private readonly DoubleArray.Builder _builder = new();

        public void Append(object? value)
        {
            if (value is null)
            {
                _builder.AppendNull();
                return;
            }

            _builder.Append(ConvertToDouble(value));
        }

        public IArrowArray Build() => _builder.Build();
    }

    private sealed class BooleanColumnBuilder : IArrowColumnBuilder
    {
        private readonly BooleanArray.Builder _builder = new();

        public void Append(object? value)
        {
            if (value is null)
            {
                _builder.AppendNull();
                return;
            }

            _builder.Append((bool)value);
        }

        public IArrowArray Build() => _builder.Build();
    }

    private sealed class Date64ColumnBuilder : IArrowColumnBuilder
    {
        private readonly Date64Array.Builder _builder = new();

        public void Append(object? value)
        {
            if (value is null)
            {
                _builder.AppendNull();
                return;
            }

            switch (value)
            {
                case DateTime dt:
                    _builder.Append(dt);
                    break;
                case DateTimeOffset dto:
                    _builder.Append(dto.UtcDateTime);
                    break;
                default:
                    throw new InvalidCastException($"Cannot convert '{value.GetType().FullName}' to Date64.");
            }
        }

        public IArrowArray Build() => _builder.Build();
    }

    private sealed class StringColumnBuilder : IArrowColumnBuilder
    {
        private readonly StringArray.Builder _builder = new();

        public void Append(object? value)
        {
            if (value is null)
            {
                _builder.AppendNull();
                return;
            }

            _builder.Append(ConvertToInvariantString(value));
        }

        public IArrowArray Build() => _builder.Build();
    }

    private sealed class NullColumnBuilder : IArrowColumnBuilder
    {
        private readonly NullArray.Builder _builder = new();

        public void Append(object? value)
        {
            _builder.AppendNull();
        }

        public IArrowArray Build() => _builder.Build();
    }

    private enum ArrowColumnKind
    {
        Null,
        Int64,
        Double,
        Boolean,
        Date64,
        String,
    }
}
