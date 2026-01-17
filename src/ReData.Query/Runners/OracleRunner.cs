using System.Data;
using System.Data.Common;
using ClickHouse.Client.Utility;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using ReData.Query.Core.Components;
using ReData.Query.Core.Value;
using ReData.Query.Runners.Value;

namespace ReData.Query.Runners;

public class OracleRunner : IQueryRunner
{
    // public required OracleConnection Connection { private get; init; }
    public required IQueryCompiler QueryCompiler { private get; init; }

    // public required IFunctionStorage FunctionStorage { private get; init; }

    // public required DatabaseValuesMapper Mapper { private get; init; }

    public async Task<DbDataReader> GetDataReaderAsync(Core.Query query, DbConnection connection)
    {
        if (connection is not OracleConnection conn)
        {
            throw new ArgumentException("Требуется OracleConnection");
        }
        
        if (connection.State is not ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        var sql = QueryCompiler.Compile(query);
        await using OracleCommand command = new OracleCommand(sql, conn);
        var reader = await command.ExecuteReaderAsync();
        return reader;
    }

    // public async Task<IReadOnlyList<Record>> RunQueryAsync(Core.Query query)
    // {
    //     var fields = query.Fields();
    //     if (Connection.State is not ConnectionState.Open)
    //     {
    //         await Connection.OpenAsync();
    //     }
    //     var result = new List<Record>();
    //     int len = query.Select?.Count ?? query.Fields().Count();
    //     var sql = QueryCompiler.Compile(query);
    //     await using var command = new OracleCommand(sql, Connection);
    //     await using OracleDataReader reader = await command.ExecuteReaderAsync();
    //     while (await reader.ReadAsync())
    //     {
    //         var current = new IValue[len];
    //         for (int i = 0; i < len; i++)
    //         {
    //             current[i] = DatabaseValuesMapper.MapField(reader.GetValue(i),fields.Get(i).Type);
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