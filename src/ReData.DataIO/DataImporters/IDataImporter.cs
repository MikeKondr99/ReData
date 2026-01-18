using System.Data.Common;

namespace ReData.DataIO.DataImporters;

public interface IDataImporter
{
    public Task<DbDataReader> ImportAsync(Stream inputStream, CancellationToken ct);
}

