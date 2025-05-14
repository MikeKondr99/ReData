using FluentAssertions;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Lang.Tests;

public class ReplaceTests
{
    
    [Theory]
    [InlineData("Func(x)  ", "x", "y", "Func(y)")]
    public void ReplaceTest(string expr, string pattern, string value, string expected)
    {
        var _expr = Expr.Parse(expr).Unwrap();
        var _pattern = Expr.Parse(pattern).Unwrap();
        var _value = Expr.Parse(value).Unwrap();
        var _expected = Expr.Parse(expected).Unwrap();

        Expr answer = _expr.Replace(_pattern, _value);
        _expr.Replace(_pattern, _value).Equivalent(_expected).Should().Be(true);
    }
    
}