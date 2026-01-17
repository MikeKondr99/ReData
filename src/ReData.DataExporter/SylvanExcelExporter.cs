using System.Data.Common;
using Sylvan.Data.Csv;
using Sylvan.Data.Excel;

namespace ReData.DataExporter;

public sealed class SylvanExcelExporter : IDataExporter
{
    public async Task ExportAsync(DbDataReader reader, Stream outputStream, CancellationToken ct)
    {
        await using var edw = await ExcelDataWriter.CreateAsync(outputStream, ExcelWorkbookType.ExcelXml, cancel: ct);
        await edw.WriteAsync(reader, "sheetName", ct);
    }
}

public sealed class SylvanCsvExporter : IDataExporter
{
    public async Task ExportAsync(DbDataReader reader, Stream outputStream, CancellationToken ct)
    {
        var textWriter = new StreamWriter(outputStream);
        await using var edw = CsvDataWriter.Create(textWriter);
        await edw.WriteAsync(reader, ct);
    }
}
            
