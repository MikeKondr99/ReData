using System.Data;
using System.Data.Common;
using Npgsql;
using ClickHouse.Client.Utility;

namespace ReData.Query.Impl.Runners;

public interface IQueryRunner : IAsyncDisposable
{
    Task<IReadOnlyList<Record>> RunQueryAsync(Query query);
    
}

public class PostgresRunner : IQueryRunner
{
    public required NpgsqlConnection Connection { private get; init; }
    public required IQueryCompiler QueryCompiler { private get; init; }
    public required IFunctionStorage FunctionStorage { private get; init; }
    public required DatabaseValuesMapper Mapper { private get; init; }
    
    public async Task<IReadOnlyList<Record>> RunQueryAsync(Query query)
    {
        if (Connection.State is not ConnectionState.Open)
        {
            await Connection.OpenAsync();
        }
        var result = new List<Record>();
        int len = query.Select?.Count ?? query.Fields(FunctionStorage).Fields.Count;
        var sql = QueryCompiler.Compile(query);
        await using NpgsqlCommand command = new NpgsqlCommand(sql, Connection);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var current = new IValue[len];
            for (int i = 0; i < len; i++)
            {
                current[i] = Mapper.MapField(reader.GetValue(i));
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