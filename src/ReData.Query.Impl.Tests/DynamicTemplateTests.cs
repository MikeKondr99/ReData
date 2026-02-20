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

    [Fact(DisplayName = "Локальная литеральная константа должна быть доступна только внутри скрипта")]
    public void ResolveScript_UsesLocalLiteralConstant()
    {
        var (resolver, context) = BuildContext();
        var script = Expr.ParseScript("const a = 10; a + 1").Unwrap();

        var resolved = resolver.ResolveScript(script, context);

        resolved.Should().NotBeNull();
        resolved!.Constants.Should().ContainKey("a");
        context.Constants.Should().NotContainKey("a");
        context.Errors.Should().BeEmpty();
    }

    [Fact(DisplayName = "Повторное объявление локальной константы с именем из глобального контекста должно завершаться ошибкой")]
    public void ResolveScript_ShouldFailWhenConstantDuplicatesGlobalOne()
    {
        var (resolver, context) = BuildContext();
        context.Constants["a"] = QueryConstant.FromValue("a", new IntegerValue(100));
        var script = Expr.ParseScript("const a = 10; a + 1").Unwrap();

        var resolved = resolver.ResolveScript(script, context);

        resolved.Should().BeNull();
        context.Errors.Should().ContainSingle();
        context.Errors[0].Message.Should().Contain("уже существует");
    }

    [Fact(DisplayName = "Сложная константа должна завершаться ошибкой при отключенном runtime")]
    public void ResolveScript_ShouldFailWhenComplexConstantRuntimeIsDisabled()
    {
        var (resolver, context) = BuildContext();
        context = context with
        {
            ConstantQuerySource = new ReData.Query.Core.Query()
            {
                Name = resolver.ResolveName(["q"]),
            },
        };
        var script = Expr.ParseScript("const a = 1 + 2; a + 1").Unwrap();
        var resolved = resolver.ResolveScript(script, context);
        resolved.Should().BeNull();
        context.Errors.Should().ContainSingle();
        var expectedError = DisabledConstantRuntime.Instance
            .Resolve(new QueryConstant()
            {
                Name = "a",
            })
            .UnwrapErr();
        context.Errors[0].Message.Should().Be(expectedError);
    }
    [Fact(DisplayName = "Ссылка на константу, объявленную ниже, должна завершаться ошибкой")]
    public void ResolveScript_ShouldFailWhenConstantReferencesFutureDeclaration()
    {
        var (resolver, context) = BuildContext();
        var script = Expr.ParseScript("const a = c; const c = 1; a").Unwrap();
        var resolved = resolver.ResolveScript(script, context);
        resolved.Should().BeNull();
        context.Errors.Should().ContainSingle();
        context.Errors[0].Message.Should().Contain("'c'");
    }
    [Fact(DisplayName = "Константно вычислимое выражение в константы без QuerySource должно завершаться ошибкой")]
    public void ResolveScript_ShouldFailWhenNonLiteralConstConstantAndConstantQuerySourceIsMissing()
    {
        var (resolver, context) = BuildContext();
        var script = Expr.ParseScript("const a = 1 + 2; a + 1").Unwrap();
        var resolved = resolver.ResolveScript(script, context);
        resolved.Should().BeNull();
        context.Errors.Should().ContainSingle();
        context.Errors[0].Message.Should().Contain("'a'");
    }
    [Fact(DisplayName = "Агрегатная константа без QuerySource должна завершаться ошибкой")]
    public void ResolveScript_ShouldFailWhenAggregatedConstantAndConstantQuerySourceIsMissing()
    {
        var (resolver, context) = BuildContext();
        var script = Expr.ParseScript("const a = AVG(a); a").Unwrap();
        var resolved = resolver.ResolveScript(script, context);
        resolved.Should().BeNull();
        context.Errors.Should().ContainSingle();
        context.Errors[0].Message.Should().Contain("'a'");
    }
    [Fact(DisplayName = "При наличии QuerySource сложная константа с отключенным runtime должна завершаться ошибкой runtime")]
    public void ResolveScript_ShouldFailWithDisabledRuntimeWhenConstantQuerySourceIsProvided()
    {
        var (resolver, context) = BuildContext();
        context = context with
        {
            ConstantQuerySource = new ReData.Query.Core.Query()
            {
                Name = resolver.ResolveName(["q"]),
            },
        };
        var script = Expr.ParseScript("const a = 1 + 2; a + 1").Unwrap();
        var resolved = resolver.ResolveScript(script, context);
        resolved.Should().BeNull();
        context.Errors.Should().ContainSingle();
        var expectedError = DisabledConstantRuntime.Instance
            .Resolve(new QueryConstant()
            {
                Name = "a",
            })
            .UnwrapErr();
        context.Errors[0].Message.Should().Be(expectedError);
    }
    [Fact(DisplayName = "Константно вычислимое выражение в константы должно успешно резолвиться при включенном runtime")]
    public void ResolveScript_ShouldResolveNonLiteralConstConstantWhenRuntimeEnabled()
    {
        var (resolver, context) = BuildContext();
        var runtime = new StubConstantRuntime(new IntegerValue(3));
        context = context with
        {
            ConstantRuntime = runtime,
            ConstantQuerySource = new ReData.Query.Core.Query()
            {
                Name = resolver.ResolveName(["q"]),
            },
        };
        var script = Expr.ParseScript("const a = 1 + 2; a + 1").Unwrap();

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

    [Fact(DisplayName = "При совпадении имени константы и поля должен использоваться приоритет константы")]
    public void ResolveExpr_ShouldPreferConstantOverFieldWhenNamesConflict()
    {
        var (resolver, context) = BuildContext();
        context.Constants["a"] = QueryConstant.FromValue("a", new IntegerValue(10));

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
            Constants = new Dictionary<string, QueryConstant>(),
            ConstantRuntime = DisabledConstantRuntime.Instance,
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

    private sealed class StubConstantRuntime(IValue resolvedValue) : IConstantRuntime
    {
        public int CreateCalls { get; private set; }

        public int ResolveCalls { get; private set; }

        public QueryConstant Create(string name, ReData.Query.Core.Query query, ResolvedExpr resolvedExpr)
        {
            CreateCalls++;
            return new QueryConstant
            {
                Name = name,
                Query = query,
            };
        }

        public Result<IValue, string> Resolve(QueryConstant constant)
        {
            ResolveCalls++;
            return Result.Ok(resolvedValue);
        }
    }
}

