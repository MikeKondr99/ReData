using FluentAssertions;
using FluentAssertions.Equivalency;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Lang.Tests;

public class StringLiteralTests
{

    private Func<EquivalencyAssertionOptions<Expr>, EquivalencyAssertionOptions<Expr>> _options = (options) =>
        options.Excluding(e => e.Span);
    
    [Theory]
    [InlineData("'{Name}:{10}'","Text(Name) + ':' + Text(10)")]
    [InlineData("'{a}1{b}2'", "Text(a) + '1' + Text(b) + '2'")]
    [InlineData("'Hello{Name}'", "'Hello' + Text(Name)")]
    [InlineData("'{Val}%'", "Text(Val) + '%'")]
    [InlineData("'{X}+{Y}={Sum}'", "Text(X) + '+' + Text(Y) + '=' + Text(Sum)")]
    [InlineData("'Count: {Count}'", "'Count: ' + Text(Count)")]
    [InlineData("'{A}{B}{C}'", "Text(A) + Text(B) + Text(C)")]
    [InlineData("'1{2}3{4}5'", "'1' + Text(2) + '3' + Text(4) + '5'")]
    [InlineData("'{Start}...{End}'", "Text(Start) + '...' + Text(End)")]
    [InlineData("'{Total} items'", "Text(Total) + ' items'")]
    [InlineData("'{10 + 20} items'", "Text(10 + 20) + ' items'")]
  
    public void Interpolation(string expr, string expected)
    {
        var input = Expr.Parse(expr).Unwrap();
        var expect = Expr.Parse(expected).Unwrap();
        input.Equivalent(expect).Should().Be(true);
    }
    
    [Theory]
    [InlineData("''","")]
    [InlineData("'text'","text")]
    [InlineData("'my string  '","my string  ")]
    [InlineData(@"'tab\''",@"tab'")]
    [InlineData(@"'(\\)'",@"(\)")]
    [InlineData(@"'(\t)'","(\t)")]
    [InlineData(@"'(\r)'","(\r)")]
    [InlineData(@"'(\n)'","(\n)")]
    [InlineData(@"'(\c)'","(c)")] // Не найденный Escape символ игнорируется
    [InlineData(@"'(\{age})'","({age})")]
    public void StringLiteral(string expr, string expected)
    {
        var e = Expr.Parse(expr).Unwrap();
        var expect = new StringLiteral(expected);
        e.Should().BeEquivalentTo(new StringLiteral(expected), _options);
    }
}