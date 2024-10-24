using System.Runtime.InteropServices;
using ReData.Query.Impl.QueryBuilders;

namespace ReData.Query.Impl.Tests.Functions.Math;

public abstract class Сommon(ISqlRunner runner) : ExprTests(runner)
{
    [Theory(DisplayName = "Addition")]
    [InlineData("2 + 2", 4)]
    [InlineData("2.5 + 3.5", 6.0)]
    public Task Add(string expr, object? expected) => Test(expr, expected);
    
    [Theory(DisplayName = "Substraction")]
    [InlineData("3 - 8", -5)]
    [InlineData("3.0 - 8.5", -5.5)]
    public Task Sub(string expr, object? expected) => Test(expr, expected);
    
    [Theory(DisplayName = "Unary minus")]
    [InlineData("-4", -4)]
    [InlineData("-3.", -3.0)]
    [InlineData("-0.", 0.0)]
    [InlineData("-0", 0)]
    public Task UnSub(string expr, object? expected) => Test(expr, expected);
    
    [Theory(DisplayName = "Multiplication")]
    [InlineData("-4. * 8.", -32.0)]
    [InlineData("-8 * -3", 24)]
    public Task Mul(string expr, object? expected) => Test(expr, expected);
    
    [Theory(DisplayName = "Division")]
    [InlineData("10 / 2", 5)]
    [InlineData("10 / 6", 1)]
    [InlineData("10.0 / 4.0", 10.0 / 4.0)]
    public Task Div(string expr, object? expected) => Test(expr, expected);
    
    [Theory(DisplayName = "Priority")]
    [InlineData("2 + 2 * 2", 6)]
    [InlineData("(2 + 2) * 2", 8)]
    public Task Priority(string expr, object? expected) => Test(expr, expected);
    
    
}