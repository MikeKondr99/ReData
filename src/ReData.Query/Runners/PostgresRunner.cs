using System.Data;
using System.Data.Common;
using Npgsql;
using ClickHouse.Client.Utility;
using ReData.Query.Core.Components;
using ReData.Query.Runners.Value;

namespace ReData.Query.Runners;

public class PostgresRunner : IQueryRunner
{
    public required NpgsqlConnection Connection { private get; init; }
    public required IQueryCompiler QueryCompiler { private get; init; }
    public required IFunctionStorage FunctionStorage { private get; init; }
    public required DatabaseValuesMapper Mapper { private get; init; }
    
    public async Task<IReadOnlyList<Record>> RunQueryAsync(Core.Query query)
    {
        if (Connection.State is not ConnectionState.Open)
        {
            await Connection.OpenAsync();
        }
        var fields = query.Fields().Fields;
        var result = new List<Record>();
        int len = query.Select?.Count ?? query.Fields().Fields.Count;
        var sql = QueryCompiler.Compile(query);
        await using NpgsqlCommand command = new NpgsqlCommand(sql, Connection);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var current = new IValue[len];
            for (int i = 0; i < len; i++)
            {
                var val = reader.GetValue(i);
                if (val is byte[])
                {
                    int a = 5;
                }
                current[i] = Mapper.MapField(reader.GetValue(i), fields[i].Type);
            }
            result.Add(new Record(current));
        }
        return result;
    }

    public async ValueTask DisposeAsync()
    {
        await Connection.DisposeAsync();
    }
}