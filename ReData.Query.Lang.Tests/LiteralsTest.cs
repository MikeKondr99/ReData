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
    [InlineData("0.0000000000", 0.0)]
    [InlineData("0.1234567890", 0.1234567890)]
    public void ShouldParseNumber(string input, double expected)
    {
        var expr = Expr.Parse(input);

        expr.Should().Be(new NumberLiteral(expected));
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
    
    
    
    
}