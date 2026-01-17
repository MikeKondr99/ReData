using System.Data.Common;
using MiniExcelLibs;
using ReData.DataExporter;

namespace ReData.DataExporter;

public sealed class MiniExcelExporter : IDataExporter
{
    public async Task ExportAsync(DbDataReader reader, Stream outputStream, CancellationToken ct)
    {
        await outputStream.SaveAsAsync(
            value: reader,
            printHeader: true,
            sheetName: "sheetName",
            excelType: ExcelType.XLSX,
            cancellationToken: ct
        );
    }
}