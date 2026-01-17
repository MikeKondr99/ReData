using System.Data.Common;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MiniExcelLibs.OpenXml;

namespace ReData.DataExporter;


public interface IDataExporter
{
    Task ExportAsync(DbDataReader reader, Stream outputStream, CancellationToken ct);
}
