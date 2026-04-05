using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using ReData.Query.Core.Components;

namespace ReData.Query.Executors;

public class SqlServerExecutor : IQueryExecutor
{
    public required IQueryCompiler QueryCompiler { private get; init; }

    public async Task<DomainDbDataReader> GetDataReaderAsync(Core.Query query, DbConnection connection)
    {
        if (connection is not SqlConnection conn)
        {
            throw new ArgumentException("Требуется SqlServerConnection");
        }

        if (connection.State is not ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var sql = QueryCompiler.Compile(query);
        await using SqlCommand command = new SqlCommand(sql, conn);
        var reader = await command.ExecuteReaderAsync();
        return reader.ToDomain(query.Fields());
    }
}
