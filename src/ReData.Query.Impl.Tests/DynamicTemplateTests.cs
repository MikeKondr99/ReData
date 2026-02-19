using System.Text;
using FluentAssertions;
using Pattern.Unions;
using ReData.Query.Core;
using ReData.Query.Core.Components;
using ReData.Query.Core.Components.Implementation;
using ReData.Query.Core.Template;
using ReData.Query.Core.Types;
using ReData.Query.Core.Value;
using ReData.Query.Impl.Functions;
using ReData.Query.Impl.LiteralBuilders;
using ReData.Query.Lang.Expressions;
using ReData.Query.Runners.Value;

namespace ReData.Query.Impl.Tests;

public class DynamicTemplateTests
{
    [Theory]
    [InlineData("Field(0)", "NULL")]
    [InlineData("Field(1)", "CAST(\"a\" AS VARCHAR)")]
    [InlineData("Field(2)", "\"b\"")]
    [InlineData("Field(3)", "NULL")]
    public void ResolvesFieldByIndex(string expr, string expectedSql)
    {
        var (resolver, context) = BuildContext();
        var sql = ResolveSql(expr, resolver, context);

        sql.Should().Be(expectedSql);
        context.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Field('a')", "CAST(\"a\" AS VARCHAR)")]
    [InlineData("Field('b')", "\"b\"")]
    [InlineData("Field('missing')", "NULL")]
    public void ResolvesFieldByName(string expr, string expectedSql)
    {
        var (resolver, context) = BuildContext();
        var sql = ResolveSql(expr, resolver, context);

        sql.Should().Be(expectedSql);
        context.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Field(1 + 1)", "Функция Field требует что бы первый аргумент был константой")]
    [InlineData("Field(a)", "Функция Field требует что бы первый аргумент был константой")]
    [InlineData("Field(-1)", "Функция Field требует что бы первый аргумент был константой")]
    public void ReturnsConstError(string expr, string expectedError)
    {
        var (resolver, context) = BuildContext();
        var resolved = resolver.ResolveExpr(Expr.Parse(expr).Unwrap(), context);

        resolved.Should().BeNull();
        context.Errors.Should().ContainSingle().Which.Message.Should().Be(expectedError);
    }

    [Theory]
    [InlineData("Field(true)")]
    [InlineData("Field(null)")]
    [InlineData("Field(1.5)")]
    public void ReturnsFunctionNotFoundError(string expr)
    {
        var (resolver, context) = BuildContext();
        var resolved = resolver.ResolveExpr(Expr.Parse(expr).Unwrap(), context);

        resolved.Should().BeNull();
        context.Errors.Should().ContainSingle().Which.Message.Should().Contain("Функция Field");
    }

    [Fact(DisplayName = "Локальная литеральная переменная должна быть доступна только внутри скрипта")]
    public void ResolveScript_UsesLocalLiteralVariable()
    {
        var (resolver, context) = BuildContext();
        var script = Expr.ParseScript("var a = 10; a + 1").Unwrap();

        var resolved = resolver.ResolveScript(script, context);

        resolved.Should().NotBeNull();
        resolved!.Variables.Should().ContainKey("a");
        context.Variables.Should().NotContainKey("a");
        context.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = "Повторное объявление локальной переменной с именем из глобального контекста должно завершаться ошибкой")]
    public void ResolveScript_ShouldFailWhenVariableDuplicatesGlobalOne()
    {
        var (resolver, context) = BuildContext();
        context.Variables["a"] = QueryVariable.FromValue("a", new IntegerValue(100));
        var script = Expr.ParseScript("var a = 10; a + 1").Unwrap();

        var resolved = resolver.ResolveScript(script, context);

        resolved.Should().BeNull();
        context.Errors.Should().ContainSingle();
        context.Errors[0].Message.Should().Contain("уже существует");
    }

    [Fact(DisplayName = "Сложная переменная должна завершаться ошибкой при отключенном runtime")]
    public void ResolveScript_ShouldFailWhenComplexVariableRuntimeIsDisabled()
    {
        var (resolver, context) = BuildContext();
        context = context with
        {
            VariableQuerySource = new ReData.Query.Core.Query()
            {
                Name = resolver.ResolveName(["q"]),
            },
        };
        var script = Expr.ParseScript("var a = 1 + 2; a + 1").Unwrap();
        var resolved = resolver.ResolveScript(script, context);
        resolved.Should().BeNull();
        context.Errors.Should().ContainSingle();
        var expectedError = DisabledVariableRuntime.Instance
            .Resolve(new QueryVariable()
            {
                Name = "a",
            })
            .UnwrapErr();
        context.Errors[0].Message.Should().Be(expectedError);
    }
    [Fact(DisplayName = "Ссылка на переменную, объявленную ниже, должна завершаться ошибкой")]
    public void ResolveScript_ShouldFailWhenVariableReferencesFutureDeclaration()
    {
        var (resolver, context) = BuildContext();
        var script = Expr.ParseScript("var a = c; var c = 1; a").Unwrap();
        var resolved = resolver.ResolveScript(script, context);
        resolved.Should().BeNull();
        context.Errors.Should().ContainSingle();
        context.Errors[0].Message.Should().Contain("'c'");
    }
    [Fact(DisplayName = "Константно вычислимое выражение в переменной без QuerySource должно завершаться ошибкой")]
    public void ResolveScript_ShouldFailWhenNonLiteralConstVariableAndVariableQuerySourceIsMissing()
    {
        var (resolver, context) = BuildContext();
        var script = Expr.ParseScript("var a = 1 + 2; a + 1").Unwrap();
        var resolved = resolver.ResolveScript(script, context);
        resolved.Should().BeNull();
        context.Errors.Should().ContainSingle();
        context.Errors[0].Message.Should().Contain("'a'");
    }
    [Fact(DisplayName = "Агрегатная переменная без QuerySource должна завершаться ошибкой")]
    public void ResolveScript_ShouldFailWhenAggregatedVariableAndVariableQuerySourceIsMissing()
    {
        var (resolver, context) = BuildContext();
        var script = Expr.ParseScript("var a = AVG(a); a").Unwrap();
        var resolved = resolver.ResolveScript(script, context);
        resolved.Should().BeNull();
        context.Errors.Should().ContainSingle();
        context.Errors[0].Message.Should().Contain("'a'");
    }
    [Fact(DisplayName = "При наличии QuerySource сложная переменная с отключенным runtime должна завершаться ошибкой runtime")]
    public void ResolveScript_ShouldFailWithDisabledRuntimeWhenVariableQuerySourceIsProvided()
    {
        var (resolver, context) = BuildContext();
        context = context with
        {
            VariableQuerySource = new ReData.Query.Core.Query()
            {
                Name = resolver.ResolveName(["q"]),
            },
        };
        var script = Expr.ParseScript("var a = 1 + 2; a + 1").Unwrap();
        var resolved = resolver.ResolveScript(script, context);
        resolved.Should().BeNull();
        context.Errors.Should().ContainSingle();
        var expectedError = DisabledVariableRuntime.Instance
            .Resolve(new QueryVariable()
            {
                Name = "a",
            })
            .UnwrapErr();
        context.Errors[0].Message.Should().Be(expectedError);
    }
    [Fact(DisplayName = "Константно вычислимое выражение в переменной должно успешно резолвиться при включенном runtime")]
    public void ResolveScript_ShouldResolveNonLiteralConstVariableWhenRuntimeEnabled()
    {
        var (resolver, context) = BuildContext();
        var runtime = new StubVariableRuntime(new IntegerValue(3));
        context = context with
        {
            VariableRuntime = runtime,
            VariableQuerySource = new ReData.Query.Core.Query()
            {
                Name = resolver.ResolveName(["q"]),
            },
        };
        var script = Expr.ParseScript("var a = 1 + 2; a + 1").Unwrap();

        var resolved = resolver.ResolveScript(script, context);

        resolved.Should().NotBeNull();
        context.Errors.Should().BeEmpty();
        runtime.CreateCalls.Should().Be(1);
        runtime.ResolveCalls.Should().Be(1);

        var compiler = new ExpressionCompiler();
        var builder = new StringBuilder();
        compiler.Compile(builder, resolved!.Expression);
        builder.ToString().Should().Contain("3");
    }

    [Fact(DisplayName = "При совпадении имени переменной и поля должен использоваться приоритет переменной")]
    public void ResolveExpr_ShouldPreferVariableOverFieldWhenNamesConflict()
    {
        var (resolver, context) = BuildContext();
        context.Variables["a"] = QueryVariable.FromValue("a", new IntegerValue(10));

        var sql = ResolveSql("a + 1", resolver, context);

        sql.Should().Contain("10");
        sql.Should().NotContain("CAST(\"a\" AS VARCHAR)");
    }

    private static (ExpressionResolver Resolver, ResolutionContext Context) BuildContext()
    {
        var literalResolver = new PostgresLiteralResolver();
        var resolver = new ExpressionResolver()
        {
            LiteralResolver = literalResolver,
        };
        var querySource = new TestQuerySource(literalResolver);
        var context = new ResolutionContext()
        {
            Errors = [],
            Functions = GlobalFunctionsStorage.GetFunctions(DatabaseTypes.PostgreSql),
            Variables = new Dictionary<string, QueryVariable>(),
            VariableRuntime = DisabledVariableRuntime.Instance,
            QuerySource = querySource,
        };
        return (resolver, context);
    }

    private static string ResolveSql(string expr, ExpressionResolver resolver, ResolutionContext context)
    {
        var resolved = resolver.ResolveExpr(Expr.Parse(expr).Unwrap(), context);
        resolved.Should().NotBeNull();

        var compiler = new ExpressionCompiler();
        var builder = new StringBuilder();
        compiler.Compile(builder, resolved!.Value);
        return builder.ToString();
    }

    private sealed class TestQuerySource : IQuerySource
    {
        private readonly IReadOnlyList<Field> fields;

        public TestQuerySource(ILiteralResolver resolver)
        {
            Name = new NameTemplate(resolver.ResolveName(["t"]).Template);
            fields =
            [
                new Field()
                {
                    Alias = "a",
                    Template = resolver.ResolveName(["a"]).Template,
                    Type = new FieldType(DataType.Integer, false),
                },
                new Field()
                {
                    Alias = "b",
                    Template = resolver.ResolveName(["b"]).Template,
                    Type = new FieldType(DataType.Text, false),
                }
            ];
        }

        public IResolvedTemplate? Name { get; }

        public IEnumerable<Field> Fields() => fields;
    }

    private sealed class StubVariableRuntime(IValue resolvedValue) : IVariableRuntime
    {
        public int CreateCalls { get; private set; }

        public int ResolveCalls { get; private set; }

        public QueryVariable Create(string name, ReData.Query.Core.Query query, ResolvedExpr resolvedExpr)
        {
            CreateCalls++;
            return new QueryVariable
            {
                Name = name,
                Query = query,
            };
        }

        public Result<IValue, string> Resolve(QueryVariable variable)
        {
            ResolveCalls++;
            return Result.Ok(resolvedValue);
        }
    }
}

