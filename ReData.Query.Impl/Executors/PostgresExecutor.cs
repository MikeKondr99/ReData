using System.Data.Common;
using Npgsql;

namespace ReData.Query.Impl.Executors;

public class PostgresExecutor
{
    // public required NpgsqlConnection Connection { private get; init; }
    //
    // public async IAsyncEnumerable<dynamic> QueryAsync(string sql)
    // {
    //     await using var command = new NpgsqlCommand(sql, Connection);
    //     await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
    //     while (await reader.ReadAsync())
    //     {
    //         dynamic row = new { };
    //         row["lol"] = reader.GetValue()
    //     }
    // }
    //
    // public 
}