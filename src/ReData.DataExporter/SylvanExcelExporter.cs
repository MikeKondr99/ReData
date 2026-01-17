using System.Data.Common;
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