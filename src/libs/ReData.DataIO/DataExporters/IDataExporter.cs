using System.Data.Common;

namespace ReData.DataIO.DataExporters;


public interface IDataExporter
{
    Task ExportAsync(DbDataReader reader, Stream outputStream, CancellationToken ct);
}
