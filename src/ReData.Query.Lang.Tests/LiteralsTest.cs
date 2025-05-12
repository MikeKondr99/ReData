using FluentAssertions;
using FluentAssertions.Equivalency;
using ReData.Query.Core;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Lang.Tests;

public class LiteralsTest
{
    private Func<EquivalencyAssertionOptions<Expr>, EquivalencyAssertionOptions<Expr>> _options = (options) =>
        options.Excluding(e => e.Span);

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
        var e = Expr.Parse(expr).Unwrap();
        e.Should().BeEquivalentTo(new NameExpr(expected), _options);
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
        var e = Expr.Parse(expr).UnwrapOk().Value;
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
        var expr = Expr.Parse(input).UnwrapOk().Value;

        expr.Should().BeEquivalentTo(new NumberLiteral(expected), _options);
    }
    
    [Fact]
    public void ShouldParseUnary()
    {
        var expr = Expr.Parse("-1").UnwrapOk().Value;


        expr.Should().BeEquivalentTo(new FuncExpr()
        {
            Name = "-",
            Arguments = [new IntegerLiteral(1)],
            Kind = FuncExprKind.Unary,
        }, _options);
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
        var expr = Expr.Parse(input).Unwrap();
        expr.Should().BeEquivalentTo(new IntegerLiteral(expected), _options);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void BooleanLiteral(string input, bool expected)
    {
        var expr = Expr.Parse(input).UnwrapOk().Value;

        expr.Should().BeEquivalentTo(new BooleanLiteral(expected), _options);
    }
    
    [Theory]
    [InlineData("null")]
    public void NullLiteral(string input)
    {
        var expr = Expr.Parse(input).UnwrapOk().Value;

        expr.Should().BeEquivalentTo(new NullLiteral(), _options);
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
        // action.Should().Throw<ParseException>()
        //     .WithMessage(message)
        //     .Where(ex => ex.Column == index);
    }
    
    [Theory]
    [InlineData("#")]
    [InlineData("a % 3")]
    public void ShouldThrowUnexpectedToken(string input)
    {
        var action = Expr.Parse(input);

        action.Should().BeNull();
    }
    
    
}