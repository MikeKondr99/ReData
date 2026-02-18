using FluentAssertions;
using FluentAssertions.Equivalency;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Lang.Tests;

public class ParsingTests
{
    private Func<EquivalencyAssertionOptions<Expr>, EquivalencyAssertionOptions<Expr>> options = (options) =>
        options.Excluding(e => e.Span);
    
    [Fact]
    public void BinaryOp()
    {
        var expr = Expr.Parse("number + 3").UnwrapOk().Value;

        expr.Should().BeEquivalentTo(
            new FuncExpr()
        {
            Name = "+",
            Kind = FuncExprKind.Binary,
            Arguments =
            [
                new NameExpr("number"),
                new IntegerLiteral(3),
            ]
        }, options);
    }
    
    [Fact]
    public void ShouldGivePriority()
    {
        var expr = Expr.Parse("a + b * c").UnwrapOk().Value;

        expr.Should().BeEquivalentTo(new FuncExpr()
        {
            Name = "+",
            Kind = FuncExprKind.Binary,
            Arguments =
            [
                new NameExpr("a"),
                new FuncExpr()
                {
                    Name = "*",
                    Arguments =
                    [
                        new NameExpr("b"),
                        new NameExpr("c"),
                    ]
                }
            ]
        }, options);
    }
    
    
    [Fact]
    public void ShouldParseWithoutCapturingBinary()
    {
        var expr = Expr.Parse("a + c.Call()").UnwrapOk().Value;


        expr.Should().BeEquivalentTo(new FuncExpr()
        {
            Name = "+",
            Kind = FuncExprKind.Binary,
            Arguments =
            [
                new NameExpr("a"),
                new FuncExpr()
                {
                    Name = "Call",
                    Arguments = [
                        new NameExpr("c")
                    ]
                }
            ]
        }, options);

    }
    
    [Fact]
    public void ShouldParseStringNonGreedy()
    {
        var expr = Expr.Parse("'a' + 'b'").UnwrapOk().Value;

        expr.Should().BeEquivalentTo(new FuncExpr()
        {
            Name = "+",
            Kind = FuncExprKind.Binary,
            Arguments =
            [
                new StringLiteral("a"),
                new StringLiteral("b"),
            ]
        }, options);

    }
    
    [Fact]
    public void ShouldParseNameNonGreedy()
    {
        var expr = Expr.Parse("[a] + [b]").UnwrapOk().Value;


        expr.Should().BeEquivalentTo(new FuncExpr()
        {
            Name = "+",
            Kind = FuncExprKind.Binary,
            Arguments =
            [
                new NameExpr("a"),
                new NameExpr("b"),
            ]
        }, options);
    }
    
    [Fact]
    public void ShouldParseStringNonGreedyInArguments()
    {
        var expr = Expr.Parse("If(10 > 5 and null, 'then', 'else')").UnwrapOk().Value;

        expr.Should().BeEquivalentTo(new FuncExpr()
        {
            Name = "If",
            Arguments =
            [
                new FuncExpr()
                {
                    Name = "and",
                    Arguments = [
                        new FuncExpr()
                        {
                            Name = ">",
                            Arguments = [
                                new IntegerLiteral(10),
                                new IntegerLiteral(5),
                            ]
                        },
                        new NullLiteral(),
                    ]
                },
                new StringLiteral("then"),
                new StringLiteral("else"),
            ]
        }, options);

    }

    [Fact]
    public void ShouldIgnoreVariableDeclarationsAndReturnFinalExpression()
    {
        var expr = Expr.Parse("var a = 12; var b = 'x'; 1 + 2").UnwrapOk().Value;
        expr.ToString().Should().Be("(1 + 2)");
    }

    [Fact]
    public void ShouldAllowAnyExpressionInVariableValue()
    {
        var expr = Expr.Parse("var a = 1 + 2; 3").UnwrapOk().Value;
        expr.ToString().Should().Be("3");
    }

    [Fact]
    public void ShouldExposeVariablesInScriptResponse()
    {
        var script = Expr.ParseScript("var a = 1 + 2; var b = AVG(age); a + b").UnwrapOk().Value;

        script.Variables.Should().HaveCount(2);
        script.Variables[0].Name.Should().Be("a");
        script.Variables[0].Expression.ToString().Should().Be("(1 + 2)");
        script.Variables[1].Name.Should().Be("b");
        script.Variables[1].Expression.ToString().Should().Be("AVG([age])");
        script.Expression.ToString().Should().Be("([a] + [b])");
    }
}
