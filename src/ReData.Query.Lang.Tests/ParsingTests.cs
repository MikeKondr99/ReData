using FluentAssertions;
using FluentAssertions.Equivalency;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Lang.Tests;

public class ParsingTests
{
    private Func<EquivalencyAssertionOptions<Expr>, EquivalencyAssertionOptions<Expr>> options = (options) =>
        options.Excluding(e => e.Span);
    
    [Fact(DisplayName = "Парсер должен строить бинарное выражение для сложения имени и константы")]
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
    
    [Fact(DisplayName = "Парсер должен учитывать приоритет операций умножения над сложением")]
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
    
    
    [Fact(DisplayName = "Парсер не должен захватывать бинарный оператор внутрь объектного вызова")]
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
    
    [Fact(DisplayName = "Парсер должен корректно разбирать строковые литералы в бинарном выражении")]
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
    
    [Fact(DisplayName = "Парсер должен корректно разбирать blocked-name в бинарном выражении")]
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
    
    [Fact(DisplayName = "Парсер должен корректно разбирать строки внутри аргументов функции")]
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

    [Fact(DisplayName = "При парсинге скрипта должны игнорироваться объявления переменных и возвращаться финальное выражение")]
    public void ShouldIgnoreVariableDeclarationsAndReturnFinalExpression()
    {
        var expr = Expr.Parse("var a = 12; var b = 'x'; 1 + 2").UnwrapOk().Value;
        expr.ToString().Should().Be("(1 + 2)");
    }

    [Fact(DisplayName = "В объявлении переменной должно поддерживаться любое выражение")]
    public void ShouldAllowAnyExpressionInVariableValue()
    {
        var expr = Expr.Parse("var a = 1 + 2; 3").UnwrapOk().Value;
        expr.ToString().Should().Be("3");
    }

    [Fact(DisplayName = "Парсер скрипта должен возвращать список объявленных переменных")]
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

    [Fact(DisplayName = "Объявление переменной через blocked_name должно завершаться ошибкой парсинга")]
    public void VariableDeclarationWithBlockedNameShouldFail()
    {
        var result = Expr.ParseScript("var [a] = 1; [a]");

        result.UnwrapErr().Message.Should().NotBeNullOrWhiteSpace();
    }
}
