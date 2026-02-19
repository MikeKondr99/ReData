using System.Globalization;
using FluentAssertions;
using ReData.Common;
using ReData.Query.Core;
using ReData.Query.Core.Value;
using ReData.Query.Impl.Tests.Fixtures;
using ReData.Query.Runners;
using ReData.Query.Runners.Value;

namespace ReData.Query.Impl.Tests;

public abstract class ExprTests(IDatabaseFixture db)
{
    
    public async Task Test(string expr, object? expected)
    {
        Console.WriteLine($"{db.GetDatabaseType()}: {expr}");
        PrepareBoolTests(ref expected, ref expr);
        var runner = await db.GetRunnerAsync();
        var connection = db.GetConnection();
        var variableRuntime = new RunnerVariableRuntime(runner, connection);
        QueryBuilder qb = QueryBuilder.FromDual(
            Factory.CreateExpressionResolver(db.GetDatabaseType()),
            Factory.CreateFunctionStorage(db.GetDatabaseType()),
            variableRuntime);
        qb = qb.Select(new()
        {
            ["test"] = expr,
        }).Expect(e => e.Select(l => l.JoinBy("\n")).JoinBy("\n\n"));
        var query = qb.Build();
        IValue result = await runner.GetDataReaderAsync(query, connection).CollectToScalar();
        Compare(result, ExpectedValue(expected));
    }

    public async Task<IValue> GetScalarAsync(QueryBuilder qb)
    {
        var runner = await db.GetRunnerAsync();
        var query = qb.Build();
        // Act
        IValue result = await runner.GetDataReaderAsync(query, db.GetConnection()).CollectToScalar();
        return result;
    }
    
    public async Task<IReadOnlyList<Dictionary<string, IValue>>> GetObjectsAsync(QueryBuilder qb)
    {
        var runner = await db.GetRunnerAsync();
        var query = qb.Build();
        // Act
        var result = await runner.GetDataReaderAsync(query, db.GetConnection()).CollectToObjects();
        return result;
    }

    private static void PrepareBoolTests(ref object? expected, ref string input)
    {
        if (expected is bool b)
        {
            input = $"If({input}, 1, 0)";
            expected = b ? 1 : 0;

        }
    }

    public static void Compare(IValue result, IValue expected)
    {
        if (result is NumberValue(var res) && expected is NumberValue(var exp))
        {
            res.Should().BeApproximately(exp, 8);
        }
        else
        {
            result.Should().Be(expected);
        }
    }

    protected static IValue ExpectedValue(object? value) => value switch
    {
        int v => new IntegerValue(v),
        long l => new IntegerValue(l),
        bool b => new BoolValue(b),
        double v => new NumberValue(v),
        string v when v.StartsWith('@') => new DateTimeValue(DateTime.Parse(v[1..],CultureInfo.InvariantCulture,DateTimeStyles.AssumeUniversal).ToUniversalTime()),
        string v => new TextValue(v),
        null => default(NullValue),
        _ => new UnknownValue(value.GetType().Name),
    };
}
