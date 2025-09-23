using FluentAssertions;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Lang.Tests;

public class EquivalencyTests
{
    
    [Theory]
    [InlineData("field", "field")]
    [InlineData("name", "[name]")]
    [InlineData("12", "012")]
    [InlineData("15.5", "015.5")]
    [InlineData("15.5", "15.500")]
    [InlineData("true", "true")]
    [InlineData("(-x)", "-x")]
    [InlineData("Func(x)", "x.Func()")]
    [InlineData("Test(x)", "Test([x])")]
    [InlineData("[x] - y", "x - [y]")]
    [InlineData("x + y + z", "(x + y) + z")]
    [InlineData("x ^ y ^ z", "x ^ (y ^ z)")]
    [InlineData("Func(x.Year(),y,x.Year())", "Func(x.Year(),y,x.Year())")]
    public void EqualsTest(string expr1, string expr2)
    {
        var exp1 = ExprNode.Parse(expr1).Unwrap();
        var exp2 = ExprNode.Parse(expr2).Unwrap();

        exp1.Equivalent(exp2).Should().Be(true);
    }
    
    [Theory]
    [InlineData("[field]", "[ field]")]
    [InlineData("12", "12.0")]
    [InlineData("'true'", "true")]
    [InlineData("'12'", "12")]
    [InlineData("'12'", "12.0")]
    [InlineData("'true'", "[true]")]
    [InlineData("x + y", "y + x")]
    [InlineData("F(x)", "f(x)")]
    [InlineData("x + y + z", "x + (y + z)")]
    [InlineData("x ^ y ^ z", "(x ^ y) ^ z")]
    public void NotEqualsTest(string expr1, string expr2)
    {
        var exp1 = ExprNode.Parse(expr1).Unwrap();
        var exp2 = ExprNode.Parse(expr2).Unwrap();

        exp1.NotEquivalent(exp2).Should().Be(true);
    }
    
}