using System.Data.Common;
using Sylvan.Data.Csv;

namespace ReData.DataExporter;

public sealed class SylvanCsvExporter : IDataExporter
{
    public async Task ExportAsync(DbDataReader reader, Stream outputStream, CancellationToken ct)
    {
        var textWriter = new StreamWriter(outputStream);
        await using var edw = CsvDataWriter.Create(textWriter);
        await edw.WriteAsync(reader, ct);
    }
}