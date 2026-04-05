using System.Data.Common;

namespace ReData.DataIO.DataImporters;

public interface IDataAnalyzer
{
    public Task<DbDataReader> AnalyzeAsync(DbDataReader reader, CancellationToken ct);
}