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
}