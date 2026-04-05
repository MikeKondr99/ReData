using System.Data;
using System.Data.Common;
using ClickHouse.Client.Utility;
using ReData.Query.Core.Components;

namespace ReData.Query.Executors;

public class ClickHouseExecutor : IQueryExecutor
{
    public required IQueryCompiler QueryCompiler { private get; init; }

    public async Task<DomainDbDataReader> GetDataReaderAsync(Core.Query query, DbConnection connection)
    {
        if (connection.State is not ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var sql = QueryCompiler.Compile(query);
        var reader = await connection.ExecuteReaderAsync(sql);
        return reader.ToDomain(query.Fields());
    }
}
