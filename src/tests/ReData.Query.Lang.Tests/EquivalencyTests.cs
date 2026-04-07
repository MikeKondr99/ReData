using ReData.Query.Lang.Expressions;

namespace ReData.Query.Lang.Tests;

public class EquivalencyTests
{

    [Test]
    [Arguments("field", "field")]
    [Arguments("name", "[name]")]
    [Arguments("12", "012")]
    [Arguments("15.5", "015.5")]
    [Arguments("15.5", "15.500")]
    [Arguments("true", "true")]
    [Arguments("(-x)", "-x")]
    [Arguments("Func(x)", "x.Func()")]
    [Arguments("Test(x)", "Test([x])")]
    [Arguments("[x] - y", "x - [y]")]
    [Arguments("x + y + z", "(x + y) + z")]
    [Arguments("x ^ y ^ z", "x ^ (y ^ z)")]
    [Arguments("Func(x.Year(),y,x.Year())", "Func(x.Year(),y,x.Year())")]
    public async Task EqualsTest(string expr1, string expr2)
    {
        var exp1 = Expr.Parse(expr1).Unwrap();
        var exp2 = Expr.Parse(expr2).Unwrap();

        await Assert.That(exp1.Equivalent(exp2)).IsTrue();
    }

    [Test]
    [Arguments("[field]", "[ field]")]
    [Arguments("12", "12.0")]
    [Arguments("'true'", "true")]
    [Arguments("'12'", "12")]
    [Arguments("'12'", "12.0")]
    [Arguments("'true'", "[true]")]
    [Arguments("x + y", "y + x")]
    [Arguments("F(x)", "f(x)")]
    [Arguments("x + y + z", "x + (y + z)")]
    [Arguments("x ^ y ^ z", "(x ^ y) ^ z")]
    public async Task NotEqualsTest(string expr1, string expr2)
    {
        var exp1 = Expr.Parse(expr1).Unwrap();
        var exp2 = Expr.Parse(expr2).Unwrap();

        await Assert.That(exp1.NotEquivalent(exp2)).IsTrue();
    }

}