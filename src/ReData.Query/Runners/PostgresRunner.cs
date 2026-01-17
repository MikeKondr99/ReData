using System.Data;
using System.Data.Common;
using System.Diagnostics;
using Npgsql;
using ClickHouse.Client.Utility;
using ReData.Query.Core.Components;
using ReData.Query.Core.Types;
using ReData.Query.Core.Value;
using ReData.Query.Runners.Value;

namespace ReData.Query.Runners;

public sealed class PostgresRunner : IQueryRunner
{
    public required IQueryCompiler QueryCompiler { private get; init; }
    
    // public required DatabaseValuesMapper Mapper { private get; init; }
    
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
        return reader;
    }

    // public async Task<IReadOnlyList<Record>> RunQueryAsync(Core.Query query)
    // {
    //     using var span = Tracing.Source.StartActivity("Postgres RunQuery");
    //     if (Connection.State is not ConnectionState.Open)
    //     {
    //         await Connection.OpenAsync();
    //         span?.AddEvent(new ActivityEvent("Connection opened"));
    //     }
    //     
    //     
    //
    //     var fields = query.Fields();
    //     var result = new List<Record>();
    //     int len = query.Select?.Count ?? query.Fields().Count();
    //     var sql = QueryCompiler.Compile(query);
    //     span?.AddEvent(new ActivityEvent("Query compiled"));
    //     await using NpgsqlCommand command = new NpgsqlCommand(sql, Connection);
    //     await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
    //     span?.AddEvent(new ActivityEvent("Query executed"));
    //     while (await reader.ReadAsync())
    //     {
    //         var current = new IValue[len];
    //         for (int i = 0; i < len; i++)
    //         {
    //             var val = reader.GetValue(i);
    //             current[i] = DatabaseValuesMapper.MapField(reader.GetValue(i), fields.Get(i).Type);
    //         }
    //
    //         result.Add(new Record(current));
    //     }
    //
    //     span?.AddEvent(new ActivityEvent("Data read complete"));
    //     return result;
    // }

    // public async ValueTask DisposeAsync()
    // {
    //     await Connection.DisposeAsync();
    // }

}