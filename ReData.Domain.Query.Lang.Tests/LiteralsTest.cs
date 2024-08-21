using System.Linq.Expressions;
using FluentAssertions;
using ReData.Domain.Query.Lang.Expressions;

namespace ReData.Domain.Query.Lang.Tests;

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

        expr.Should().Be(new StringLiteral()
        {
            Value = "my string   "
        });
    }
    
    [Fact]
    public void StringCanHaveTabs()
    {
        var expr = Expr.Parse("'my string   \t'");

        expr.Should().Be(new StringLiteral()
        {
            Value = "my string   \t"
        });
    }
    
    [Theory]
    [InlineData("1.3",1.3)]
    [InlineData("0.0", 0.0)]
    [InlineData("0.0000000000", 0.0)]
    [InlineData("0.1234567890", 0.1234567890)]
    public void ShouldParseNumber(string input, double expected)
    {
        var expr = Expr.Parse(input);

        expr.Should().Be(new NumberLiteral()
        {
            Value = expected
        });
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

        expr.Should().Be(new IntegerLiteral()
        {
            Value = expected
        });
    }

    [Theory]
    [InlineData("1.\n3")]
    public void NewLineShouldInterupt(string input)
    {
        var sut = () => Expr.Parse(input);

        sut.Should().Throw<Exception>();
    }
    
    
    
    
}