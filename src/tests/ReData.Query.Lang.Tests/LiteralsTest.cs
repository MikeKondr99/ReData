using ReData.Query.Lang.Expressions;

namespace ReData.Query.Lang.Tests;

public class LiteralsTest
{

    [Test]
    [Arguments("name", "name")]
    [Arguments("[first name]", "first name")]
    [Arguments("[ first name  ]", " first name  ")]
    [Arguments(@"[arr[i\]]", "arr[i]")]
    [Arguments("[*?carl$$]", "*?carl$$")]
    [Arguments(@"[\]", @"\")]
    [Arguments(@"[name\]", @"name\")]
    [Arguments("[\"Quote\" me]", "\"Quote\" me")]
    [Arguments("[null]", "null")]
    [Arguments("[true]", "true")]
    [Arguments("[false]", "false")]
    [Arguments("[and]", "and")]
    public async Task NameLiteral(string expr, string expected)
    {
        var e = Expr.Parse(expr).Unwrap();
        await Assert.That(e.Equivalent(new NameExpr(expected))).IsTrue();
    }

    [Test]
    [Arguments("''", "")]
    [Arguments("'text'", "text")]
    [Arguments("'my string  '", "my string  ")]
    // [Arguments("'tab\t'","tab\t")]
    // [Arguments(@"'tab\n'","tab\n")]
    // [Arguments(@"'tab\r'","tab\r")]
    // [Arguments(@"'tab\''","tab'")]
    // [Arguments(@"'ta\' '","ta'")]
    [Arguments(@"'tab\''", @"tab'")]
    // [Arguments(@"' \\n '",@" \n ")]
    public async Task StringLiteral(string expr, string expected)
    {
        var e = Expr.Parse(expr).Unwrap();

        await Assert.That(e.Equivalent(new StringLiteral(expected))).IsTrue();
    }

    [Test]
    [Arguments("1.3", 1.3)]
    [Arguments("0.0", 0.0)]
    [Arguments(".3", .3)]
    [Arguments("5.0", 5.0)]
    [Arguments("0.0000000000", 0.0)]
    [Arguments("0.1234567890", 0.1234567890)]
    public async Task NumberLiteral(string input, double expected)
    {
        var expr = Expr.Parse(input).UnwrapOk().Value;

        await Assert.That(expr.Equivalent(new NumberLiteral(expected))).IsTrue();
    }

    [Test]
    public async Task ShouldParseUnary()
    {
        var expr = Expr.Parse("-1").UnwrapOk().Value;

        await Assert.That(expr.Equivalent(new FuncExpr()
        {
            Name = "-",
            Arguments = [new IntegerLiteral(1)],
            Kind = FuncExprKind.Unary,
        })).IsTrue();
    }

    [Test]
    [Arguments("0", 0)]
    [Arguments("10", 10)]
    [Arguments("123", 123)]
    [Arguments("4567", 4567)]
    [Arguments("9999", 9999)]
    [Arguments("5678", 5678)]
    [Arguments("00001", 1)]
    public async Task IntegerLiteral(string input, long expected)
    {
        var expr = Expr.Parse(input).Unwrap();
        await Assert.That(expr.Equivalent(new IntegerLiteral(expected))).IsTrue();
    }

    [Test]
    [Arguments("true", true)]
    [Arguments("false", false)]
    public async Task BooleanLiteral(string input, bool expected)
    {
        var expr = Expr.Parse(input).UnwrapOk().Value;

        await Assert.That(expr.Equivalent(new BooleanLiteral(expected))).IsTrue();
    }

    [Test]
    [Arguments("null")]
    public async Task NullLiteral(string input)
    {
        var expr = Expr.Parse(input).UnwrapOk().Value;

        await Assert.That(expr.Equivalent(new NullLiteral())).IsTrue();
    }

    // [Test]
    // [Arguments("()", "expected expression", 1)]
    // [Arguments("2 +", "expected expression", 3)]
    // [Arguments("* 3", "expected expression", 0)]
    // [Arguments("a + 3)", "expected end of expression", 5)]
    // [Arguments("(a + 3", "expected ')'", 6)]
    // [Arguments("2,3", "expected end of expression",1)]
    // [Arguments("f(1,2", "expected ',' or ')'",5)]
    // [Arguments("f(x,)", "expected expression",4)]
    // [Arguments("f(x,", "expected expression",4)]
    // [Arguments("12(x)", "expected end of expression",2)]
    // [Arguments("+3", "expected expression",0)]
    // public async Task  ShouldNotParse(string input, string message, int index)
    // {
    //     Expr.Parse(input).UnwrapErr();
    // }

    [Test]
    [Arguments("#")]
    [Arguments("a % 3")]
    public async Task ShouldThrowUnexpectedToken(string input)
    {
        Expr.Parse(input).UnwrapErr();
    }


}