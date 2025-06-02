using FluentAssertions;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Lang.Tests;

public class ReplaceTests
{
    
    [Theory]
    [InlineData("Func(x)  ", "x", "y", "Func(y)")]
    public void ReplaceTest(string expr, string pattern, string value, string expected)
    {
        var expr1 = Expr.Parse(expr).Unwrap();
        var pattern1 = Expr.Parse(pattern).Unwrap();
        var value1 = Expr.Parse(value).Unwrap();
        var expected1 = Expr.Parse(expected).Unwrap();

        Expr answer = expr1.Replace(pattern1, value1);
        expr1.Replace(pattern1, value1).Equivalent(expected1).Should().Be(true);
    }
    
}