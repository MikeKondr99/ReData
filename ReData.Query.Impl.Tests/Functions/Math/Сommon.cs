using System.Runtime.InteropServices;
using ReData.Query.Impl.QueryBuilders;

namespace ReData.Query.Impl.Tests.Functions.Math;

public abstract class Сommon(ISqlRunner runner) : ExprTests(runner)
{
    [Theory(DisplayName = "Addition")]
    [InlineData("2 + 2", 4)]
    [InlineData("2.5 + 3.5", 6.0)]
    [InlineData("2.5 + 4", 6.5)]
    [InlineData("Type(3 + 3)", "Integer")]
    [InlineData("Type(3.0 + If(true,2, null))", "Number?")]
    [InlineData("2 + null", null)]
    [InlineData("Type(2 + null)", "Integer?")]
    public Task Add(string expr, object? expected) => Test(expr, expected);
    
    [Theory(DisplayName = "Substraction")]
    [InlineData("3 - 8", -5)]
    [InlineData("3.0 - 8.5", -5.5)]
    public Task Sub(string expr, object? expected) => Test(expr, expected);
    
    [Theory(DisplayName = "Unary minus")]
    [InlineData("-4", -4)]
    [InlineData("-3.0", -3.0)]
    [InlineData("-0.0", 0.0)]
    [InlineData("-0", 0)]
    public Task UnSub(string expr, object? expected) => Test(expr, expected);
    
    [Theory(DisplayName = "Multiplication")]
    [InlineData("-4.0 * 8.0", -32.0)]
    [InlineData("-8 * -3", 24)]
    public Task Mul(string expr, object? expected) => Test(expr, expected);
    
    [Theory(DisplayName = "Division")]
    [InlineData("10 / 2", 5)]
    [InlineData("10 / 6", 1)]
    [InlineData("10.0 / 4.0", 10.0 / 4.0)]
    public Task Div(string expr, object? expected) => Test(expr, expected);
    
    [Theory(DisplayName = "Power Function")]
    // Test cases for a ^ b
    [InlineData("2 ^ 3", 8)]              // 2 raised to the power of 3
    [InlineData("3 ^ 2", 9)]              // 3 raised to the power of 2
    [InlineData("5 ^ 0", 1)]              // Any number raised to the power of 0 is 1
    [InlineData("10 ^ -1", 0.1)]          // 10 raised to the power of -1
    [InlineData("2.0 ^ 3.0", 8.0)]        // Float power
    [InlineData("2 ^ 0.5", 1.41421356237)] // Square root of 2 (approx)

    // Test cases for a.Pow(b)
    [InlineData("2.Pow(3)", 8)]          // Same as 2 ^ 3
    [InlineData("3.Pow(2)", 9)]          // Same as 3 ^ 2
    [InlineData("5.Pow(0)", 1)]          // Any number raised to the power of 0 is 1
    [InlineData("10.Pow(-1)", 0.1)]      // 10 raised to the power of -1
    [InlineData("2.0.Pow(3.0)", 8.0)]    // Float power
    [InlineData("2.Pow(0.5)", 1.41421356237)] // Square root of 2 (approx)

    // Test cases for Pow(a, b)
    [InlineData("Pow(2, 3)", 8)]          // Same as 2 ^ 3
    [InlineData("Pow(3, 2)", 9)]          // Same as 3 ^ 2
    [InlineData("Pow(5, 0)", 1)]          // Any number raised to the power of 0 is 1
    [InlineData("Pow(10, -1)", 0.1)]      // 10 raised to the power of -1
    [InlineData("Pow(2.0, 3.0)", 8.0)]    // Float power
    [InlineData("Pow(2, 0.5)", 1.41421356237)] // Square root of 2 (approx)

    // Test cases for right associativity and operator precedence
    [InlineData("2 ^ 3 + 5", 8 + 5)]      // Power takes precedence over addition
    [InlineData("2 + 3 ^ 3", 2 + 27)]      // Right associativity of power
    [InlineData("2 ^ 3 * 2", 8 * 2)]      // Power takes precedence over multiplication
    [InlineData("2 * 3 ^ 2", 2 * 9)]      // Right associativity of power
    [InlineData("2 ^ 3 ^ 2", 512)]        // Right associativity (2^(3^2) == 2^9)
    [InlineData("2.Pow(3 + 1)", 16)]      // Power with parenthesis in addition
    [InlineData("Pow(2, 3 * 2)", 64)]     // Power with multiplication in the exponent

    public Task Power(string expr, object? expected) => Test(expr, expected);
    
    
    [Theory(DisplayName = "Priority")]
    [InlineData("2 + 2 * 2", 6)]
    [InlineData("(2 + 2) * 2", 8)]
    public Task Priority(string expr, object? expected) => Test(expr, expected);
    
    
    [Theory(DisplayName = "Between")]
    [InlineData("Between(5, 2, 10)", true)]
    [InlineData("5.Between(2, 10)", true)]
    [InlineData("Between(2, 5, 10)", false)]
    [InlineData("2.Between(5, 10)", false)]
    public Task Between(string expr, object? expected) => Test(expr, expected);
    
}