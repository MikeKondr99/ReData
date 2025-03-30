using FluentAssertions;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Lang.Tests;

public class ParsingTests
{
    [Fact]
    public void BinaryOp()
    {
        var expr = RawExpr.Parse("number + 3");

        expr.Should().BeEquivalentTo(new FuncRawExpr()
        {
            Name = "+",
            Kind = FuncExprKind.Binary,
            Arguments =
            [
                new NameRawExpr("number"),
                new RawIntegerLiteral(3),
            ]
        });
    }
    
    [Fact]
    public void ShouldGivePriority()
    {
        var expr = RawExpr.Parse("a + b * c");

        expr.Should().BeEquivalentTo(new FuncRawExpr()
        {
            Name = "+",
            Kind = FuncExprKind.Binary,
            Arguments =
            [
                new NameRawExpr("a"),
                new FuncRawExpr()
                {
                    Name = "+",
                    Arguments =
                    [
                        new NameRawExpr("b"),
                        new NameRawExpr("c"),
                    ]
                }
            ]
        });
    }
    
    
    [Fact]
    public void ShouldParseWithoutCapturingBinary()
    {
        var expr = RawExpr.Parse("a + c.Call()");


        expr.Should().BeEquivalentTo(new FuncRawExpr()
        {
            Name = "+",
            Kind = FuncExprKind.Binary,
            Arguments =
            [
                new NameRawExpr("a"),
                new FuncRawExpr()
                {
                    Name = "Call",
                    Arguments = [
                        new NameRawExpr("c")
                    ]
                }
            ]
        });

    }
    
    [Fact]
    public void ShouldParseStringNonGreedy()
    {
        var expr = RawExpr.Parse("'a' + 'b'");

        expr.Should().BeEquivalentTo(new FuncRawExpr()
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
        var expr = RawExpr.Parse("[a] + [b]");


        expr.Should().BeEquivalentTo(new FuncRawExpr()
        {
            Name = "+",
            Kind = FuncExprKind.Binary,
            Arguments =
            [
                new NameRawExpr("a"),
                new NameRawExpr("b"),
            ]
        });
    }
    
    [Fact]
    public void ShouldParseStringNonGreedyInArguments()
    {
        var expr = RawExpr.Parse("If(10 > 5 and null, 'then', 'else')");

        expr.Should().BeEquivalentTo(new FuncRawExpr()
        {
            Name = "If",
            Arguments =
            [
                new FuncRawExpr()
                {
                    Name = "and",
                    Arguments = [
                        new FuncRawExpr()
                        {
                            Name = ">",
                            Arguments = [
                                new RawIntegerLiteral(10),
                                new RawIntegerLiteral(5),
                            ]
                        },
                        new RawNullRawLiteral(),
                    ]
                },
                new StringLiteral("then"),
                new StringLiteral("else"),
            ]
        });

    }
}