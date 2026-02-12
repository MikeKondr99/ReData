using System.Data;
using System.Data.Common;
using MySql.Data.MySqlClient;
using ReData.Query.Core.Components;

namespace ReData.Query.Runners;

public class MySqlRunner : IQueryRunner
{
    public required IQueryCompiler QueryCompiler { private get; init; }

  
    public async Task<DbDataReader> GetDataReaderAsync(Core.Query query, DbConnection connection)
    {
        if (connection is not MySqlConnection conn)
        {
            throw new ArgumentException("Требуется MySqlConnection");
        }
      
        if (connection.State is not ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var sql = QueryCompiler.Compile(query);
        await using MySqlCommand command = new MySqlCommand(sql, conn);
        var reader = await command.ExecuteReaderAsync();
        return new ClrMappedDbDataReader(reader, query.Fields());
    }
}
