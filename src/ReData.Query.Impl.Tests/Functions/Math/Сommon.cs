using System.Runtime.InteropServices;
using ReData.Query.Impl.Tests.Fixtures;

namespace ReData.Query.Impl.Tests.Functions.Math;

public abstract class Сommon(IDatabaseFixture runner) : ExprTests(runner)
{
    [Theory(DisplayName = "Addition")]
    [InlineData("2 + 2", 4)]
    [InlineData("2.5 + 3.5", 6.0)]
    [InlineData("2.5 + 4", 6.5)]
    [InlineData("Type(3 + 3)", "int!")]
    [InlineData("Type(3.0 + If(true,2, null))", "num")]
    [InlineData("2 + null", null)]
    [InlineData("Type(2 + null)", "int")]
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

    // [Theory(DisplayName = "Power Function")]
    // // Test cases for a ^ b
    // [InlineData("2 ^ 3", 8)]              // 2 raised to the power of 3
    // [InlineData("3 ^ 2", 9)]              // 3 raised to the power of 2
    // [InlineData("5 ^ 0", 1)]              // Any number raised to the power of 0 is 1
    // [InlineData("10 ^ -1", 0.1)]          // 10 raised to the power of -1
    // [InlineData("2.0 ^ 3.0", 8.0)]        // Float power
    // [InlineData("2 ^ 0.5", 1.41421356237)] // Square root of 2 (approx)
    //
    // // Test cases for a.Pow(b)
    // [InlineData("2.Pow(3)", 8)]          // Same as 2 ^ 3
    // [InlineData("3.Pow(2)", 9)]          // Same as 3 ^ 2
    // [InlineData("5.Pow(0)", 1)]          // Any number raised to the power of 0 is 1
    // [InlineData("10.Pow(-1)", 0.1)]      // 10 raised to the power of -1
    // [InlineData("2.0.Pow(3.0)", 8.0)]    // Float power
    // [InlineData("2.Pow(0.5)", 1.41421356237)] // Square root of 2 (approx)
    //
    // // Test cases for Pow(a, b)
    // [InlineData("Pow(2, 3)", 8)]          // Same as 2 ^ 3
    // [InlineData("Pow(3, 2)", 9)]          // Same as 3 ^ 2
    // [InlineData("Pow(5, 0)", 1)]          // Any number raised to the power of 0 is 1
    // [InlineData("Pow(10, -1)", 0.1)]      // 10 raised to the power of -1
    // [InlineData("Pow(2.0, 3.0)", 8.0)]    // Float power
    // [InlineData("Pow(2, 0.5)", 1.41421356237)] // Square root of 2 (approx)
    //
    // // Test cases for right associativity and operator precedence
    // [InlineData("2 ^ 3 + 5", 8 + 5)]      // Power takes precedence over addition
    // [InlineData("2 + 3 ^ 3", 2 + 27)]      // Right associativity of power
    // [InlineData("2 ^ 3 * 2", 8 * 2)]      // Power takes precedence over multiplication
    // [InlineData("2 * 3 ^ 2", 2 * 9)]      // Right associativity of power
    // [InlineData("2 ^ 3 ^ 2", 512)]        // Right associativity (2^(3^2) == 2^9)
    // [InlineData("2.Pow(3 + 1)", 16)]      // Power with parenthesis in addition
    // [InlineData("Pow(2, 3 * 2)", 64)]     // Power with multiplication in the exponent
    //
    // public Task Power(string expr, object? expected) => Test(expr, expected);


    [Theory(DisplayName = "Priority")]
    [InlineData("2 + 2 * 2", 6)]
    [InlineData("(2 + 2) * 2", 8)]
    [InlineData("(-10 + 1).Sign()", -1)]
    [InlineData("-10 + 1.Sign()", -9)]
    public Task Priority(string expr, object? expected) => Test(expr, expected);

    [SkippableTheory(DisplayName = "E (Euler's Number)")]
    [InlineData("E()", 2.718281828459045)] // Exact value of e
    [InlineData("E() > 2.71", true)] // e is greater than 2.71
    [InlineData("E() < 2.72", true)] // e is less than 2.72
    [InlineData("E() + 1", 3.718281828459045)] // e + 1
    [InlineData("E() * 2", 5.43656365691809)] // e * 2
    [InlineData("E() / 2", 1.3591409142295225)] // e / 2
    public Task EFunction(string expr, object? expected)
    {
        Skip.If(expr is "E() * 2" && runner.GetDatabaseType() is DatabaseType.PostgreSql);
        return Test(expr, expected);
    }

    [SkippableTheory(DisplayName = "Pi (π)")]
    [InlineData("Pi()", 3.141592653589793)] // Exact value of π
    [InlineData("Pi() > 3.14", true)] // π is greater than 3.14
    [InlineData("Pi() < 3.15", true)] // π is less than 3.15
    [InlineData("Pi() + 1", 4.141592653589793)] // π + 1
    [InlineData("Pi() * 2", 6.283185307179586)] // π * 2
    [InlineData("Pi() / 2", 1.5707963267948966)] // π / 2
    public Task PiFunction(string expr, object? expected)
    {
        Skip.If(expr is "Pi() * 2" && runner.GetDatabaseType() is DatabaseType.PostgreSql);
        return Test(expr, expected);
    }

    [Theory(DisplayName = "Cos (Radians)")]
    [InlineData("Cos(0)", 1.0)]
    [InlineData("Cos(1.0471975511965976)", 0.5)] // ~60°
    [InlineData("Cos(1.5707963267948966)", 0.0)] // ~90°
    public Task Cos(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "Sin (Radians)")]
    [InlineData("Sin(0)", 0.0)]
    [InlineData("Sin(0.5235987755982988)", 0.5)] // ~30°
    [InlineData("Sin(1.5707963267948966)", 1.0)] // ~90°
    public Task Sin(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "Tan (Radians)")]
    [InlineData("Tan(0)", 0.0)]
    [InlineData("Tan(0.7853981633974483)", 1.0)] // ~45°
    public Task Tan(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "Acos(Inverse Cos)")]
    [InlineData("Acos(1)", 0.0)]
    [InlineData("Acos(-0.5)", 1.0471975511965976)]
    [InlineData("Acos(0.5)", 1.0471975511965976)] // ~60°
    // Недопустимые значения
    [InlineData("Acos(1.1)", null)] // >1
    [InlineData("Acos(999)", null)] // Далеко за пределами
    public Task Acos(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "Asin(Inverse Sin)")]
    [InlineData("Asin(0)", 0.0)]
    [InlineData("Asin(-0.5)", -0.5235987755982988)]
    [InlineData("Asin(0.5)", 0.5235987755982988)] // ~30°
    [InlineData("Asin(-1.0)", -1.5707963267948966)]
    // Недопустимые значения
    [InlineData("Asin(-1.1)", null)] // <-1
    [InlineData("Asin(999)", null)] // Далеко за пределами
    public Task Asin(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "Atan(Inverse Tangent)")]
    [InlineData("Atan(0)", 0.0)]
    [InlineData("Atan(1)", 0.7853981633974483)] // ~45°
    public Task Atan(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "Atan2")]
    [InlineData("Atan2(1, 1)", 0.7853981633974483)] // ~45°
    [InlineData("Atan2(-1, -1)", -2.356194490192345)] // ~-135°
    public Task Atan2(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "Atanh")]
    [InlineData("Atanh(0.5)", 0.5493061443340548)]
    [InlineData("Atanh(-0.99)", -2.6466524123622457)]
    [InlineData("Atanh(1.0)", null)]
    [InlineData("Atanh(-1.0)", null)]
    [InlineData("Atanh(1.1)", null)]
    public Task AtanhRangeValidation(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "Valid Non-Range Functions")]
    // Функции без ограничений по входу
    [InlineData("Cos(999)", -0.8537530939092378)] // Любое число
    [InlineData("Sin(-123.456)", -0.267690548907)] // Любое число
    [InlineData("Tan(3.1415926535897931)", 0.0)] // π (почти 0)
    public Task NonRangeTrigFunctions(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "Radians (Degrees to Radians)")]
    [InlineData("Rad(0)", 0.0)]
    [InlineData("Rad(180)", System.Math.PI)]
    [InlineData("Rad(360)", 2 * System.Math.PI)]
    [InlineData("Rad(90)", System.Math.PI / 2)]
    public Task Radians(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "Degrees (Radians to Degrees)")]
    [InlineData("Deg(0)", 0.0)]
    [InlineData("Deg(3.1415926535897931)", 180.0)] // π → 180°
    [InlineData("Deg(6.2831853071795862)", 360.0)] // 2π → 360°
    [InlineData("Deg(1.5707963267948966)", 90.0)] // π/2 → 90°
    public Task Degrees(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "Cosh (Hyperbolic Cos)")]
    [InlineData("Cosh(0)", 1.0)]
    [InlineData("Cosh(1)", 1.5430806348152437)]
    [InlineData("Cosh(-1)", 1.5430806348152437)] // Cosh(-x) = Cosh(x)
    public Task Cosh(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "Sinh (Hyperbolic Sin)")]
    [InlineData("Sinh(0)", 0.0)]
    [InlineData("Sinh(1)", 1.1752011936438014)]
    [InlineData("Sinh(-1)", -1.1752011936438014)] // Sinh(-x) = -Sinh(x)
    public Task Sinh(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "Tanh (Hyperbolic Tan)")]
    [InlineData("Tanh(0)", 0.0)]
    [InlineData("Tanh(1)", 0.7615941559557649)]
    [InlineData("Tanh(-1)", -0.7615941559557649)] // Tanh(-x) = -Tanh(x)
    [InlineData("Tanh(100)", 1.0)] // При больших значениях стремится к 1
    public Task Tanh(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "Acosh (Inverse Hyperbolic Cos)")]
    [InlineData("Acosh(1)", 0.0)]
    [InlineData("Acosh(2)", 1.3169578969248166)]
// Недопустимые значения
    [InlineData("Acosh(0.5)", null)] // x должен быть >= 1
    [InlineData("Acosh(-1)", null)]
    public Task Acosh(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "Asinh (Inverse Hyperbolic Sin)")]
    [InlineData("Asinh(0)", 0.0)]
    [InlineData("Asinh(1)", 0.881373587019543)]
    [InlineData("Asinh(-1)", -0.881373587019543)] // Asinh(-x) = -Asinh(x)
// Нет ограничений по входу
    [InlineData("Asinh(999)", 7.30686074806869)]
    [InlineData("Asinh(-999)", -7.30686074806869)]
    public Task Asinh(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "Edge Cases for All Functions")]
    [InlineData("Cos(null)", null)]
    [InlineData("Sin(null)", null)]
    [InlineData("Tan(null)", null)]
    [InlineData("Acos(null)", null)]
    [InlineData("Asin(null)", null)]
    [InlineData("Atan(null)", null)]
    [InlineData("Atan2(null, 1)", null)]
    [InlineData("Atan2(1, null)", null)]
    [InlineData("Cosh(null)", null)]
    [InlineData("Sinh(null)", null)]
    [InlineData("Tanh(null)", null)]
    [InlineData("Acosh(null)", null)]
    [InlineData("Asinh(null)", null)]
    [InlineData("Atanh(null)", null)]
    [InlineData("Rad(null)", null)]
    [InlineData("Deg(null)", null)]
    public Task NullInputTests(string expr, object? expected) => Test(expr, expected);
}