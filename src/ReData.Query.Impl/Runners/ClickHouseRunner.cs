using System.Data;
using System.Data.Common;
using ClickHouse.Client.ADO;
using ClickHouse.Client.ADO.Readers;
using ClickHouse.Client.Utility;
using Npgsql;

namespace ReData.Query.Impl.Runners;

public class ClickHouseRunner : IQueryRunner
{
    public required ClickHouseConnection  Connection { private get; init; }
    public required IQueryCompiler QueryCompiler { private get; init; }
    public required IFunctionStorage FunctionStorage { private get; init; }
    public required DatabaseValuesMapper Mapper { private get; init; }
    
    public async Task<IReadOnlyList<Record>> RunQueryAsync(Query query)
    {
        var fields = query.Fields();
        if (Connection.State is not ConnectionState.Open)
        {
            await Connection.OpenAsync();
        }
        var result = new List<Record>();
        int len = query.Select?.Count ?? query.Fields().Fields.Count;
        var sql = QueryCompiler.Compile(query);
        await using DbDataReader reader = await Connection.ExecuteReaderAsync(sql);
        while (await reader.ReadAsync())
        {
            var current = new IValue[len];
            for (int i = 0; i < len; i++)
            {
                current[i] = Mapper.MapField(reader.GetValue(i),fields[i].Type);
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