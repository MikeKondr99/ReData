using System.Data;
using System.Data.Common;
using Npgsql;
using ClickHouse.Client.Utility;

namespace ReData.Query.Impl.Runners;

public interface IQueryRunner : IAsyncDisposable
{
    Task<IReadOnlyList<Record>> RunQueryAsync(Query query);

    
    async Task<IReadOnlyList<Dictionary<string, IValue>>> RunQueryAsObjectAsync(Query query)
    {
        var data = await RunQueryAsync(query);
        var fields = query.Fields().Fields.Select(f => f.Alias).ToList();
    
        List<Dictionary<string, IValue>> result = new List<Dictionary<string, IValue>>();
    
        foreach (var record in data)
        {
            var recordDict = new Dictionary<string, IValue>();
        
            for (int i = 0; i < fields.Count; i++)
            {
                // Assuming record.values is an IList<IValue> or similar
                if (i < record.values.Length)
                {
                    recordDict[fields[i]] = record.values[i];
                }
            }
        
            result.Add(recordDict);
        }
    
        return result;
    }
    
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
        var fields = query.Fields();
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