using FluentAssertions;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Lang.Tests;

public class ParsingTests
{
    [Fact]
    public void BinaryOp()
    {
        var expr = Expr.Parse("number + 3");

        expr.Should().BeEquivalentTo(new FuncExpr()
        {
            Name = "+",
            Kind = FuncExprKind.Binary,
            Arguments =
            [
                new NameExpr("number"),
                new IntegerLiteral(3),
            ]
        });
    }
    
    [Fact]
    public void ShouldGivePriority()
    {
        var expr = Expr.Parse("a + b * c");

        expr.Should().BeEquivalentTo(new FuncExpr()
        {
            Name = "+",
            Kind = FuncExprKind.Binary,
            Arguments =
            [
                new NameExpr("a"),
                new FuncExpr()
                {
                    Name = "+",
                    Arguments =
                    [
                        new NameExpr("b"),
                        new NameExpr("c"),
                    ]
                }
            ]
        });
    }
    
    
    [Fact]
    public void ShouldParseWithoutCapturingBinary()
    {
        var expr = Expr.Parse("a + c.Call()");


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
        });

    }
    
    [Fact]
    public void ShouldParseStringNonGreedy()
    {
        var expr = Expr.Parse("'a' + 'b'");

        expr.Should().BeEquivalentTo(new FuncExpr()
        {
            Name = "+",
            Kind = FuncExprKind.Binary,
            Arguments =
            [
                new StringLiteral("a"),
                new StringLiteral("b"),
            ]
        });

    }
    
    [Fact]
    public void ShouldParseNameNonGreedy()
    {
        var expr = Expr.Parse("[a] + [b]");


        expr.Should().BeEquivalentTo(new FuncExpr()
        {
            Name = "+",
            Kind = FuncExprKind.Binary,
            Arguments =
            [
                new NameExpr("a"),
                new NameExpr("b"),
            ]
        });
    }
    
    [Fact]
    public void ShouldParseStringNonGreedyInArguments()
    {
        var expr = Expr.Parse("If(10 > 5 and null, 'then', 'else')");

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
        });

    }
}