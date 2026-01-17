using System.Data;
using System.Data.Common;
using MySql.Data.MySqlClient;
using ReData.Query.Core.Components;
using ReData.Query.Core.Value;
using ReData.Query.Runners.Value;

namespace ReData.Query.Runners;

public class MySqlRunner : IQueryRunner
{
    public required IQueryCompiler QueryCompiler { private get; init; }
    // public required DatabaseValuesMapper Mapper { private get; init; }

    
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
        return reader;
    }
    // public async Task<IReadOnlyList<Record>> RunQueryAsync(Core.Query query)
    // {
    //     if (Connection.State is not ConnectionState.Open)
    //     {
    //         await Connection.OpenAsync();
    //     }
    //
    //     var fields = query.Fields();
    //     var result = new List<Record>();
    //     int len = query.Select?.Count ?? query.Fields().Count();
    //     var sql = QueryCompiler.Compile(query);
    //     await using var command = new MySqlCommand(sql, Connection);
    //     await using DbDataReader reader = await command.ExecuteReaderAsync();
    //     while (await reader.ReadAsync())
    //     {
    //         var current = new IValue[len];
    //         for (int i = 0; i < len; i++)
    //         {
    //             current[i] = DatabaseValuesMapper.MapField(reader.GetValue(i), fields.Get(i).Type);
    //         }
    //         result.Add(new Record(current));
    //     }
    //     return result;
    // }
    //
    // public async ValueTask DisposeAsync()
    // {
    //     await Connection.DisposeAsync();
    // }
}