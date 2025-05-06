using System.Globalization;
using FluentAssertions;
using ReData.Query.Core;
using ReData.Query.Impl.Tests.Fixtures;
using ReData.Query.Runners;
using ReData.Query.Runners.Value;

namespace ReData.Query.Impl.Tests;

public abstract class ExprExtensionTests(IDatabaseFixture db)
{
    private static DatabaseValuesMapper Mapper = new DatabaseValuesMapper();
    private static Factory Factory = new Factory();
    
    public async Task Test(string expr, object? expected)
    {
        var runner = await db.GetRunnerAsync();
        PrepareBoolTests(ref expected, ref expr);
        QueryBuilder qb = QueryBuilder.FromDual(Factory.CreateExpressionResolver(db.GetDatabaseType()));
        qb = qb.Select(new()
        {
            ["test"] = expr,
        }).UnwrapOk().Value;
        var result = await runner.SingleAsync(qb.Build());
        Compare(result,ExpectedValue(expected));
    }

    private static void PrepareBoolTests(ref object? expected, ref string input)
    {
        if (expected is bool b)
        {
            input = $"If({input}, 1, 0)";
            expected = b ? 1 : 0;

        }
    }

    private void Compare(IValue result, IValue expected)
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

    protected IValue ExpectedValue(object? value) => value switch
    {
        int v => new IntegerValue(v),
        bool b => new BoolValue(b),
        double v => new NumberValue(v),
        string v when v.StartsWith("@") => new DateTimeValue(DateTime.Parse(v[1..],CultureInfo.InvariantCulture,DateTimeStyles.AssumeLocal).ToUniversalTime()),
        string v => new TextValue(v),
        null => new NullValue(),
        _ => new UnknownValue(value.GetType().Name),
    };

    internal readonly struct BoolNull;
}


file static class QueryRunnerExtensions
{
    public async static Task<IValue> SingleAsync(this IQueryRunner runner, Core.Query query)
    {
        var data = await runner.RunQueryAsync(query);
        return data.Single()[0];
    }
    
}