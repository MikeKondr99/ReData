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

    [Fact(DisplayName = "При парсинге скрипта должны игнорироваться объявления констант и возвращаться финальное выражение")]
    public void ShouldIgnoreConstantDeclarationsAndReturnFinalExpression()
    {
        var expr = Expr.Parse("const a = 12; const b = 'x'; 1 + 2").UnwrapOk().Value;
        expr.ToString().Should().Be("(1 + 2)");
    }

    [Fact(DisplayName = "В объявлении константы должно поддерживаться любое выражение")]
    public void ShouldAllowAnyExpressionInConstantValue()
    {
        var expr = Expr.Parse("const a = 1 + 2; 3").UnwrapOk().Value;
        expr.ToString().Should().Be("3");
    }

    [Fact(DisplayName = "Вызов const(...) должен парситься как обычная функция")]
    public void ShouldParseInlineConstFunction()
    {
        var expr = Expr.Parse("const(1 + 2)").UnwrapOk().Value;

        expr.Should().BeEquivalentTo(new FuncExpr()
        {
            Name = "const",
            Kind = FuncExprKind.Default,
            Arguments =
            [
                new FuncExpr()
                {
                    Name = "+",
                    Kind = FuncExprKind.Binary,
                    Arguments =
                    [
                        new IntegerLiteral(1),
                        new IntegerLiteral(2),
                    ]
                }
            ]
        }, options);
    }

    [Fact(DisplayName = "Парсер скрипта должен возвращать список объявленных констант")]
    public void ShouldExposeConstantsInScriptResponse()
    {
        var script = Expr.ParseScript("const a = 1 + 2; const b = AVG(age); a + b").UnwrapOk().Value;
        var constants = script.GetConstantDeclarations();
        var macros = script.GetMacroDeclarations();

        script.Declarations.Should().HaveCount(2);
        constants.Should().HaveCount(2);
        macros.Should().BeEmpty();
        constants[0].Name.Should().Be("a");
        constants[0].Expression.ToString().Should().Be("(1 + 2)");
        constants[1].Name.Should().Be("b");
        constants[1].Expression.ToString().Should().Be("AVG([age])");
        script.Expression.ToString().Should().Be("([a] + [b])");
    }

    [Fact(DisplayName = "Парсер скрипта должен возвращать let-декларации в Macros")]
    public void ShouldExposeMacrosInScriptResponse()
    {
        var script = Expr.ParseScript("let isAdult = [age] > 18; const a = 1; a").UnwrapOk().Value;
        var macros = script.GetMacroDeclarations();
        var constants = script.GetConstantDeclarations();

        script.Declarations.Should().HaveCount(2);
        macros.Should().HaveCount(1);
        macros[0].Kind.Should().Be(ScriptDeclarationKind.Macro);
        macros[0].Name.Should().Be("isAdult");
        macros[0].Expression.ToString().Should().Be("([age] > 18)");
        constants.Should().HaveCount(1);
        constants[0].Kind.Should().Be(ScriptDeclarationKind.Const);
        constants[0].Name.Should().Be("a");
    }

    [Fact(DisplayName = "Объявление константы через blocked_name должно завершаться ошибкой парсинга")]
    public void ConstantDeclarationWithBlockedNameShouldFail()
    {
        var result = Expr.ParseScript("const [a] = 1; [a]");

        result.UnwrapErr().Message.Should().NotBeNullOrWhiteSpace();
    }
}
