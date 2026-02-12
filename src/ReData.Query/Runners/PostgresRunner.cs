using System.Data;
using System.Data.Common;
using Npgsql;
using ReData.Query.Core.Components;

namespace ReData.Query.Runners;

public sealed class PostgresRunner : IQueryRunner
{
    public required IQueryCompiler QueryCompiler { private get; init; }
    
    
    public async Task<DbDataReader> GetDataReaderAsync(Core.Query query, DbConnection connection)
    {
        if (connection is not NpgsqlConnection conn)
        {
            throw new ArgumentException("Требуется SqlServerConnection");
        }
        
        if (conn.State is not ConnectionState.Open)
        {
            await conn.OpenAsync();
        }

        var sql = QueryCompiler.Compile(query);
        await using NpgsqlCommand command = new NpgsqlCommand(sql, conn);
        var reader = await command.ExecuteReaderAsync();
        return reader.ClrNormalize(query.Fields());
    }
}
