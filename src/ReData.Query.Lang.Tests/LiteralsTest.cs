using System.Linq.Expressions;
using FluentAssertions;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Lang.Tests;

public class LiteralsTest
{

    [Theory]
    [InlineData("name","name")]
    [InlineData("[first name]","first name")]
    [InlineData("[ first name  ]"," first name  ")]
    [InlineData(@"[arr[i\]]","arr[i]")]
    [InlineData("[*?carl$$]","*?carl$$")]
    [InlineData(@"[\]",@"\")]
    [InlineData(@"[name\]",@"name\")]
    [InlineData("[\"Quote\" me]","\"Quote\" me")]
    [InlineData("[null]","null")]
    [InlineData("[true]","true")]
    [InlineData("[false]","false")]
    [InlineData("[and]","and")]
    public void NameLiteral(string expr, string expected)
    {
        var e = Expr.Parse(expr);
        e.Should().Be(new NameExpr(expected));
    }
    
    [Theory]
    [InlineData("''","")]
    [InlineData("'text'","text")]
    [InlineData("'my string  '","my string  ")]
    // [InlineData("'tab\t'","tab\t")]
    // [InlineData(@"'tab\n'","tab\n")]
    // [InlineData(@"'tab\r'","tab\r")]
    // [InlineData(@"'tab\''","tab'")]
    // [InlineData(@"'ta\' '","ta'")]
    [InlineData(@"'tab\'",@"tab\")]
    // [InlineData(@"' \\n '",@" \n ")]
    public void StringLiteral(string expr, string expected)
    {
        var e = Expr.Parse(expr);
        e.Should().Be(new StringLiteral(expected));
    }
    
    [Theory]
    [InlineData("1.3",1.3)]
    [InlineData("0.0", 0.0)]
    [InlineData(".3", .3)]
    [InlineData("5.0", 5.0)]
    [InlineData("0.0000000000", 0.0)]
    [InlineData("0.1234567890", 0.1234567890)]
    public void NumberLiteral(string input, double expected)
    {
        var expr = Expr.Parse(input);

        expr.Should().Be(new NumberLiteral(expected));
    }
    
    [Fact]
    public void ShouldParseUnary()
    {
        var expr = Expr.Parse("-1");


        expr.Should().BeEquivalentTo(new FuncExpr()
        {
            Name = "-",
            Arguments = [new IntegerLiteral(1)],
            Kind = FuncExprKind.Unary,
        });
    }
    
    [Theory]
    [InlineData("0", 0)]
    [InlineData("123", 123)]
    [InlineData("4567", 4567)]
    [InlineData("9999", 9999)]
    [InlineData("5678", 5678)]
    [InlineData("00001", 1)]
    public void IntegerLiteral(string input, long expected)
    {
        var expr = Expr.Parse(input);
        expr.Should().Be(new IntegerLiteral(expected));
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void BooleanLiteral(string input, bool expected)
    {
        var expr = Expr.Parse(input);

        expr.Should().Be(new BooleanLiteral(expected));
    }
    
    [Theory]
    [InlineData("null")]
    public void NullLiteral(string input)
    {
        var expr = Expr.Parse(input);

        expr.Should().Be(new NullLiteral());
    }
    
    [Theory]
    // [InlineData("()", "expected expression", 1)]
    // [InlineData("2 +", "expected expression", 3)]
    [InlineData("* 3", "expected expression", 0)]
    // [InlineData("a + 3)", "expected end of expression", 5)]
    // [InlineData("(a + 3", "expected ')'", 6)]
    // [InlineData("2,3", "expected end of expression",1)]
    // [InlineData("f(1,2", "expected ',' or ')'",5)]
    // [InlineData("f(x,)", "expected expression",4)]
    // [InlineData("f(x,", "expected expression",4)]
    // [InlineData("12(x)", "expected end of expression",2)]
    // [InlineData("+3", "expected expression",0)]
    public void ShouldNotParse(string input, string message, int index)
    {
        var action = () => Expr.Parse(input);
        action.Should().Throw<ParseException>()
            .WithMessage(message)
            .Where(ex => ex.Column == index);
    }
    
    [Theory]
    [InlineData("#")]
    [InlineData("a % 3")]
    public void ShouldThrowUnexpectedToken(string input)
    {
        var action = () => Expr.Parse(input);
        action.Should().Throw<UnexpectedTokenException>()
            .WithMessage("unexpected token");
    }
    
    
}