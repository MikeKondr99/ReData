using System.Diagnostics;
using System.Globalization;
using ClickHouse.Client.Numerics;
using FluentAssertions;
using ReData.Query.Impl.QueryBuilders;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Impl.Tests;

public abstract class ExprTests(ISqlRunner runner)
{
    protected ISqlRunner Runner { get; } = runner;
    
    public async Task Test(string expr, object? expected)
    {
        var input = Expr.Parse(expr);
        if (expected is bool b)
        {
            input = new FuncExpr()
            {
                Name = "If",
                Arguments = [input, new IntegerLiteral(1), new IntegerLiteral(0)]
            };
            expected = b ? 1 : 0;
        }
        Query query = new Query()
        {
            Select =
            [
                new Query.Map("test", input),
            ]
        };
        var sql = Runner.QueryBuilder.Build(query);
        var result = await Runner.Scalar(sql);
        if (expected is double exp)
        {
            if (result is ClickHouseDecimal cd)
            {
                result = cd.ToDecimal(new NumberFormatInfo());
            }
            if (result is double dd)
            {
                result = (decimal)dd;
            }
            if (result is not Decimal dec)
            {
                result.Should().BeOfType<Decimal>();
                throw new UnreachableException();
            }

            exp = Math.Round(exp, 7);
            double res = Math.Round((double)dec, 7);
            res.Should().Be(exp);
        }
        result.Should().Be(expected);
    }
}