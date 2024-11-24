using System.Collections;
using System.Data;
using System.Data.Common;
using Npgsql;
using ReData.Query.Functions;
using ReData.Query.Visitors;

namespace ReData.Query.Impl.Executors;

public class PostgresExecutor
{
    public required NpgsqlConnection Connection { private get; init; }
    
    public required IQueryBuilder QueryBuilder { private get; init; }
    public required IFunctionStorage FunctionStorage { private get; init; }
    
    public async Task<IReadOnlyList<IValue[]>> QueryAsync(Query query)
    {
        var result = new List<IValue[]>();
        var current = new IValue[query.Fields(FunctionStorage).Fields.Count];
        await using NpgsqlCommand command = new NpgsqlCommand("sql", Connection);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            current[0] = MapField(reader.GetValue(0));
        }
        return result;
    }

    private IValue MapField(object? value)
    {
        return value switch
        {
            double d => new NumberValue(d),
            float f => new NumberValue(f),
            decimal dc => new NumberValue((double)dc),
            string s => new TextValue(s),
            DBNull => new NullValue(),
            null => new NullValue(),
            _ => new UnknownValue(),
        };

    }
}

public interface IValue
{
}

public record struct TextValue(string Value) : IValue;

public record struct IntegerValue(long Value) : IValue;

public record struct NumberValue(double Value) : IValue;

public record struct BoolValue(bool Value) : IValue;

public record struct UnknownValue(string originalType) : IValue;

public record struct NullValue : IValue;
