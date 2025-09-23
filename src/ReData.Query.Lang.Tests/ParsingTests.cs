using FluentAssertions;
using FluentAssertions.Equivalency;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Lang.Tests;

public class ParsingTests
{
    private Func<EquivalencyAssertionOptions<ExprNode>, EquivalencyAssertionOptions<ExprNode>> options = (options) =>
        options.Excluding(e => e.Span);
    
    [Fact]
    public void BinaryOp()
    {
        var expr = ExprNode.Parse("number + 3").UnwrapOk().Value;

        expr.Should().BeEquivalentTo(
            new FuncExprNode()
        {
            Name = "+",
            Kind = FuncExprKind.Binary,
            Arguments =
            [
                new NameExprNode("number"),
                new IntegerLiteral(3),
            ]
        }, options);
    }
    
    [Fact]
    public void ShouldGivePriority()
    {
        var expr = ExprNode.Parse("a + b * c").UnwrapOk().Value;

        expr.Should().BeEquivalentTo(new FuncExprNode()
        {
            Name = "+",
            Kind = FuncExprKind.Binary,
            Arguments =
            [
                new NameExprNode("a"),
                new FuncExprNode()
                {
                    Name = "*",
                    Arguments =
                    [
                        new NameExprNode("b"),
                        new NameExprNode("c"),
                    ]
                }
            ]
        }, options);
    }
    
    
    [Fact]
    public void ShouldParseWithoutCapturingBinary()
    {
        var expr = ExprNode.Parse("a + c.Call()").UnwrapOk().Value;


        expr.Should().BeEquivalentTo(new FuncExprNode()
        {
            Name = "+",
            Kind = FuncExprKind.Binary,
            Arguments =
            [
                new NameExprNode("a"),
                new FuncExprNode()
                {
                    Name = "Call",
                    Arguments = [
                        new NameExprNode("c")
                    ]
                }
            ]
        }, options);

    }
    
    [Fact]
    public void ShouldParseStringNonGreedy()
    {
        var expr = ExprNode.Parse("'a' + 'b'").UnwrapOk().Value;

        expr.Should().BeEquivalentTo(new FuncExprNode()
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
        var expr = ExprNode.Parse("[a] + [b]").UnwrapOk().Value;


        expr.Should().BeEquivalentTo(new FuncExprNode()
        {
            Name = "+",
            Kind = FuncExprKind.Binary,
            Arguments =
            [
                new NameExprNode("a"),
                new NameExprNode("b"),
            ]
        }, options);
    }
    
    [Fact]
    public void ShouldParseStringNonGreedyInArguments()
    {
        var expr = ExprNode.Parse("If(10 > 5 and null, 'then', 'else')").UnwrapOk().Value;

        expr.Should().BeEquivalentTo(new FuncExprNode()
        {
            Name = "If",
            Arguments =
            [
                new FuncExprNode()
                {
                    Name = "and",
                    Arguments = [
                        new FuncExprNode()
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