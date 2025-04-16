using System.Diagnostics;
using System.Globalization;
using ClickHouse.Client.Numerics;
using FluentAssertions;
using ReData.Query.Impl.QueryBuilders;
using ReData.Query.Impl.Runners;
using ReData.Query.Impl.Runners.Value;
using ReData.Query.Impl.Tests.Fixtures;
using ReData.Query.Lang.Expressions;
using ReData.Query.Visitors;

namespace ReData.Query.Impl.Tests;

public abstract class RawExprTests(IDatabaseFixture db)
{
    private static DatabaseValuesMapper Mapper = new DatabaseValuesMapper();
    private static QueryServicesFactory Factory = new QueryServicesFactory();
    
    public async Task Test(string expr, object? expected)
    {
        var runner = await db.GetRunnerAsync();
        PrepareBoolTests(ref expected, ref expr);
        QueryBuilder qb = QueryBuilder.FromDual(Factory.CreateExpressionResolver(db.GetDatabaseType()));
        qb = qb.Select(new()
        {
            ["test"] = expr,
        });
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
        string v when v.StartsWith("@") => new DateTimeValue(DateTime.Parse(v[1..],CultureInfo.InvariantCulture,DateTimeStyles.AssumeLocal)),
        string v => new TextValue(v),
        null => new NullValue(),
        _ => new UnknownValue(value.GetType().Name),
    };

    internal readonly struct BoolNull;
}


file static class QueryRunnerExtensions
{
    public async static Task<IValue> SingleAsync(this IQueryRunner runner, Query query)
    {
        var data = await runner.RunQueryAsync(query);
        return data.Single()[0];
    }
    
}