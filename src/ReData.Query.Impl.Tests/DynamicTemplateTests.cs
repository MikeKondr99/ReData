using System.Text;
using FluentAssertions;
using ReData.Query.Core;
using ReData.Query.Core.Components;
using ReData.Query.Core.Components.Implementation;
using ReData.Query.Core.Template;
using ReData.Query.Core.Types;
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
            Variables = new Dictionary<string, IValue>(),
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
}
