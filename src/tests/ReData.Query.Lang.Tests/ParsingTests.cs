using ReData.Query.Lang.Expressions;

namespace ReData.Query.Lang.Tests;

public class ParsingTests
{
    [Test]
    [DisplayName("Парсер должен строить бинарное выражение для сложения имени и константы")]
    public async Task BinaryOp()
    {
        var expr = Expr.Parse("number + 3").UnwrapOk().Value;

        await Assert.That(expr.Equivalent(
            new FuncExpr()
            {
                Name = "+",
                Kind = FuncExprKind.Binary,
                Arguments =
            [
                new NameExpr("number"),
                new IntegerLiteral(3),
            ]
        })).IsTrue();
    }

    [Test]
    [DisplayName("Парсер должен учитывать приоритет операций умножения над сложением")]
    public async Task ShouldGivePriority()
    {
        var expr = Expr.Parse("a + b * c").UnwrapOk().Value;

        await Assert.That(expr.Equivalent(
            new FuncExpr()
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
        })).IsTrue();
    }


    [Test]
    [DisplayName("Парсер не должен захватывать бинарный оператор внутрь объектного вызова")]
    public async Task ShouldParseWithoutCapturingBinary()
    {
        var expr = Expr.Parse("a + c.Call()").UnwrapOk().Value;

        await Assert.That(expr.Equivalent(new FuncExpr()
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
        })).IsTrue();
    }

    [Test]
    [DisplayName("Парсер должен корректно разбирать строковые литералы в бинарном выражении")]
    public async Task ShouldParseStringNonGreedy()
    {
        var expr = Expr.Parse("'a' + 'b'").UnwrapOk().Value;

        await Assert.That(expr.Equivalent(new FuncExpr()
        {
            Name = "+",
            Kind = FuncExprKind.Binary,
            Arguments =
            [
                new StringLiteral("a"),
                new StringLiteral("b"),
            ]
        })).IsTrue();

    }

    [Test]
    [DisplayName("Парсер должен корректно разбирать blocked-name в бинарном выражении")]
    public async Task ShouldParseNameNonGreedy()
    {
        var expr = Expr.Parse("[a] + [b]").UnwrapOk().Value;


        await Assert.That(expr.Equivalent(new FuncExpr()
        {
            Name = "+",
            Kind = FuncExprKind.Binary,
            Arguments =
            [
                new NameExpr("a"),
                new NameExpr("b"),
            ]
        })).IsTrue();
    }

    [Test]
    [DisplayName("Парсер должен корректно разбирать строки внутри аргументов функции")]
    public async Task ShouldParseStringNonGreedyInArguments()
    {
        var expr = Expr.Parse("If(10 > 5 and null, 'then', 'else')").UnwrapOk().Value;

        await Assert.That(expr.Equivalent(new FuncExpr()
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
        })).IsTrue();

    }

    [Test]
    [DisplayName("При парсинге скрипта должны игнорироваться объявления констант и возвращаться финальное выражение")]
    public async Task ShouldIgnoreConstantDeclarationsAndReturnFinalExpression()
    {
        var expr = Expr.Parse("const a = 12; const b = 'x'; 1 + 2").UnwrapOk().Value;
        await Assert.That(expr.ToString()).IsEqualTo("(1 + 2)");
    }

    [Test]
    [DisplayName("В объявлении константы должно поддерживаться любое выражение")]
    public async Task ShouldAllowAnyExpressionInConstantValue()
    {
        var expr = Expr.Parse("const a = 1 + 2; 3").UnwrapOk().Value;
        await Assert.That(expr.ToString()).IsEqualTo("3");
    }

    [Test]
    [DisplayName("Вызов const(...) должен парситься как обычная функция")]
    public async Task ShouldParseInlineConstFunction()
    {
        var expr = Expr.Parse("const(1 + 2)").UnwrapOk().Value;

        await Assert.That(expr.Equivalent(new FuncExpr()
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
        })).IsTrue();
    }

    [Test]
    [DisplayName("Парсер скрипта должен возвращать список объявленных констант")]
    public async Task ShouldExposeConstantsInScriptResponse()
    {
        var script = Expr.ParseScript("const a = 1 + 2; const b = AVG(age); a + b").UnwrapOk().Value;
        var constants = script.GetConstantDeclarations();
        var macros = script.GetMacroDeclarations();

        await Assert.That(script.Declarations).Count().IsEqualTo(2);
        await Assert.That(constants).Count().IsEqualTo(2);
        await Assert.That(macros).IsEmpty();
        await Assert.That(constants[0])
            .Member(c => c.Name, name => name.IsEqualTo("a"))
            .And.Member(c => c.Expression.ToString(), e => e.IsEqualTo("(1 + 2)"));
        await Assert.That(constants[1])
            .Member(c => c.Name, name => name.IsEqualTo("b"))
            .And.Member(c => c.Expression.ToString(), e => e.IsEqualTo("AVG([age])"));
        await Assert.That(script.Expression.ToString()).IsEqualTo("([a] + [b])");
    }

    [Test]
    [DisplayName("Парсер скрипта должен возвращать let-декларации в Macros")]
    public async Task ShouldExposeMacrosInScriptResponse()
    {
        var script = Expr.ParseScript("let isAdult = [age] > 18; const a = 1; a").UnwrapOk().Value;
        var macros = script.GetMacroDeclarations();
        var constants = script.GetConstantDeclarations();

        await Assert.That(script.Declarations).Count().IsEqualTo(2);
        await Assert.That(macros).Count().IsEqualTo(1);
        await Assert.That(constants).Count().IsEqualTo(1);
        await Assert.That(macros[0])
            .Member(c => c.Name, name => name.IsEqualTo("isAdult"))
            .And.Member(c => c.Kind, kind => kind.IsEqualTo(ScriptDeclarationKind.Macro))
            .And.Member(c => c.Expression.ToString(), e => e.IsEqualTo("([age] > 18)"));
        await Assert.That(constants[0])
            .Member(c => c.Name, name => name.IsEqualTo("a"))
            .And.Member(c => c.Kind, kind => kind.IsEqualTo(ScriptDeclarationKind.Const));
    }

    [Test]
    [DisplayName("Объявление константы через blocked_name должно завершаться ошибкой парсинга")]
    public async Task ConstantDeclarationWithBlockedNameShouldFail()
    {
        var result = Expr.ParseScript("const [a] = 1; [a]");

        await Assert.That(result.UnwrapErr().Message).IsNotNullOrWhiteSpace();
    }
}
