using System.Diagnostics;
using System.Globalization;
using ClickHouse.Client.Numerics;
using FluentAssertions;
using ReData.Query.Impl.Runners;
using ReData.Query.Impl.Runners.Value;
using ReData.Query.Impl.Tests.Fixtures;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Impl.Tests;

public abstract class RawExprTests(IDatabaseFixture fixture)
{
    private static DatabaseValuesMapper Mapper = new DatabaseValuesMapper();
    
    public async Task Test(string expr, object? expected)
    {
        var runner = await fixture.GetRunnerAsync();
        var input = RawExpr.Parse(expr);
        expected = PrepareBoolTests(expected, ref input);
        expected = PrepareDateTimeTests(expected);
        Query query = new Query()
        {
            Select =
            [
                new Query.Map("test", input),
            ]
        };
        var result = await runner.SingleAsync(query);
        Compare(result, Mapper.MapField(expected));
    }

    private static object? PrepareDateTimeTests(object? expected)
    {
        if (expected is string s && s.StartsWith("@"))
        {
            expected = DateTimeOffset.Parse(s[1..]);
        }

        return expected;
    }

    private static object? PrepareBoolTests(object? expected, ref IExpr input)
    {
        if (expected is bool or NullBool)
        {
            if (expected is NullBool)
            {
                // Expr.Parse("{input} or true)")
                input = new FuncExpr()
                {
                    Name = "or",
                    Kind = FuncExprKind.Binary,
                    Arguments =
                    [
                        input,
                        new BooleanLiteral(true),
                    ]
                };
            }
            input = new FuncExpr()
            {
                Name = "If",
                Arguments = [input, new IntegerLiteral(1), new IntegerLiteral(0)]
            };
            expected = expected switch
            {
                bool b => b ? 1 : 0,
                NullBool => 0,
                _ => expected
            };

        }

        return expected;
    }

    public static void Compare(IValue result, IValue expected)
    {
        object? o =(result, expected) switch
        {
            (NumberValue(var res), NumberValue(var exp)) => res.Should().BeApproximately(exp, 7),
            // TODO убрать после рефактора
            (NumberValue(var res), IntegerValue(var exp)) => res.Should().BeApproximately(exp, 7),
            (IntegerValue(var res), IntegerValue(var exp)) => res.Should().Be(exp),
            (IntegerValue(var res), BoolValue(var exp)) => res.Should().Be(exp ? 1 : 0),
            (BoolValue(var res), BoolValue(var exp)) => res.Should().Be(exp),
            (TextValue(var res), TextValue(var exp)) => res.Should().Be(exp),
            (DateTimeValue(var res), DateTimeValue(var exp)) => res.Should().Be(exp),
            (_, NullValue) => result.Should().BeOfType<NullValue>(),
            (_, _) => result.Should().BeOfType(expected.GetType()),
        };

    }
    
    internal readonly struct BoolNull;

    internal const string NullBool = "εNULLε";
}


file static class QueryRunnerExtensions
{
    public async static Task<IValue> SingleAsync(this IQueryRunner runner, Query query)
    {
        var data = await runner.RunQueryAsync(query);
        return data.Single()[0];
    }
    
}