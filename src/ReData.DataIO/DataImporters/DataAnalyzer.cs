using System.Data.Common;
using ReData.DataIO.ValueFormats;

namespace ReData.DataIO.DataImporters;

public sealed class DataAnalyzer : IDataAnalyzer
{
    public async Task<DbDataReader> AnalyzeAsync(DbDataReader reader, CancellationToken ct)
    {
        List<string[]> buffer = new();
        var fieldCount = reader.FieldCount;
        List<IValueFormat>[] formats = GetInitialFormats(fieldCount);
        
        string[] columnNames = Enumerable
            .Range(0, fieldCount)
            .Select(i => reader.GetName(i))
            .ToArray();
        
        while (await reader.ReadAsync(ct))
        {
            string[] row = new string[fieldCount];
            for (int i = 0; i < fieldCount; i++)
            {
                var value = reader.GetString(i);
                row[i] = value;
                NarrowFormats(formats[i], value);
            }
            buffer.Add(row);
        }
        await reader.DisposeAsync();

        IValueFormat?[] finalFormats = formats.Select(fmts => fmts.FirstOrDefault()).ToArray();

        return new TypedDbDataReader(buffer, finalFormats, columnNames);
    }
    
    private static void NarrowFormats(List<IValueFormat> formats, string value)
    {
        if (string.IsNullOrEmpty(value))
            return;
        formats.RemoveAll(f => !f.TryConvert(value, out _));
    }

    private static List<IValueFormat>[] GetInitialFormats(int fieldCount)
    {
        return Enumerable.Range(0, fieldCount).Select(i => GetInitialFormats()).ToArray();
    }
    
    private static List<IValueFormat> GetInitialFormats()
    {
        return
        [
            new IntegerFormat(),
            new DoubleFormat(),
            new BooleanNumericFormat(),
            new BooleanTrueFalseFormat(),
            new IsoDateFormat(),
            new UtcDateFormat(),
            new StringFormat()
        ];
    }
}