using System.Linq.Expressions;
using FluentAssertions;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Lang.Tests;

public class LiteralsTest
{
    [Fact]
    public void NameShouldParse()
    {
        var expr = Expr.Parse("name");

        expr.Should().Be(new NameExpr("name"));
    }
    
    [Fact]
    public void BlockNameShouldParse()
    {
        var expr = Expr.Parse("[first name]");

        expr.Should().Be(new NameExpr("first name"));
    }
    
    [Fact]
    public void BlockNameShouldTrimSpaces()
    {
        var expr = Expr.Parse("[ first name   ]");

        expr.Should().Be(new NameExpr("first name"));
    }
    
    [Fact]
    public void BlockNameShouldTrimEmptySymbols()
    {
        var expr = Expr.Parse("[ first name  ]");

        expr.Should().Be(new NameExpr("first name"));
    }
    
    [Fact]
    public void StringShouldParse()
    {
        var expr = Expr.Parse("'my string   '");

        expr.Should().Be(new StringLiteral("my string   "));
    }
    
    [Fact]
    public void StringCanHaveTabs()
    {
        var expr = Expr.Parse("'my string  \t'");

        expr.Should().Be(new StringLiteral("my string  \t"));
    }
    
    [Theory]
    [InlineData("1.3",1.3)]
    [InlineData("0.0", 0.0)]
    [InlineData(".3", .3)]
    [InlineData("5.0", 5.0)]
    [InlineData("0.0000000000", 0.0)]
    [InlineData("0.1234567890", 0.1234567890)]
    public void ShouldParseNumber(string input, double expected)
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
        // expr.Should().BeOfType<FuncExpr>();
        // if (expr is FuncExpr f)
        // {
        //     f.Name.Should().Be("-");
        //     f.Arguments.Should().BeEquivalentTo([new IntegerLiteral(1)]);
        // }
    }
    
    [Theory]
    [InlineData("0", 0)]
    [InlineData("123", 123)]
    [InlineData("4567", 4567)]
    [InlineData("9999", 9999)]
    [InlineData("5678", 5678)]
    [InlineData("00001", 1)]
    public void ShouldParseInteger(string input, long expected)
    {
        var expr = Expr.Parse(input);
        expr.Should().Be(new IntegerLiteral(expected));
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void ShouldParseBoolean(string input, bool expected)
    {
        var expr = Expr.Parse(input);

        expr.Should().Be(new BooleanLiteral(expected));
    }
    
    [Theory]
    [InlineData("null")]
    public void ShouldParseNull(string input)
    {
        var expr = Expr.Parse(input);

        expr.Should().Be(new NullLiteral());
    }
    
    [Theory]
    [InlineData("()", "expected expression", 1)]
    [InlineData("2 +", "expected expression", 3)]
    [InlineData("* 3", "expected expression", 0)]
    [InlineData("a + 3)", "expected end of expression", 5)]
    [InlineData("(a + 3", "expected ')'", 6)]
    [InlineData("2,3", "expected end of expression",1)]
    [InlineData("f(1,2", "expected ',' or ')'",5)]
    [InlineData("f(x,)", "expected expression",4)]
    [InlineData("f(x,", "expected expression",4)]
    [InlineData("12(x)", "expected end of expression",2)]
    [InlineData("+3", "expected expression",0)]
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
    
    [Fact]
    public void ShouldPlus()
    {
        var expr = Expr.Parse("a + 3");
        expr.Should().BeEquivalentTo(new FuncExpr()
        {
            Name = "+",
            Arguments = [new NameExpr("a"), new IntegerLiteral(3)],
            Kind = FuncExprKind.Binary,
        });
    }
}