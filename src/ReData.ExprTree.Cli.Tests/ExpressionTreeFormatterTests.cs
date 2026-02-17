using FluentAssertions;
using ReData.ExprTree.Cli;
using ReData.Query.Lang.Expressions;

namespace ReData.ExprTree.Cli.Tests;

public class ExpressionTreeFormatterTests
{
    [Fact]
    public void ToDot_ShouldContainEdges_ForFunctionAndBinaryArguments()
    {
        const string source = "a + b";
        var expr = Expr.Parse(source).Unwrap();
        var sqlTrace = PostgresExpressionSqlResolver.Resolve(expr);

        var result = ExpressionTreeFormatter.ToDot(expr, source, sqlTrace);

        result.Should().Contain("digraph");
        result.Should().Contain("\"label\"=\"+\"");
        result.Should().Contain("->");
        result.Should().Contain("\"label\"=\"a + b\"");
        result.Should().Contain("source -> n0");
        result.Should().Contain("subgraph sql_tokens");
        result.Should().Contain("\"style\"=\"dashed\"");
    }

    [Fact]
    public void RenderSvg_ShouldCreateSvgFile()
    {
        const string source = "a + b";
        var expr = Expr.Parse(source).Unwrap();
        var sqlTrace = PostgresExpressionSqlResolver.Resolve(expr);
        var dot = ExpressionTreeFormatter.ToDot(expr, source, sqlTrace);
        var path = Path.Combine(Path.GetTempPath(), $"expr-tree-test-{Guid.NewGuid():N}.svg");

        try
        {
            GraphvizRenderer.RenderSvg(dot, path);
            File.Exists(path).Should().BeTrue();
            File.ReadAllText(path).Should().Contain("<svg");
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void ToDot_ShouldLetGraphvizChooseMethodPorts()
    {
        const string source = "'text'.KeepChars('mask')";
        var expr = Expr.Parse(source).Unwrap();

        var result = ExpressionTreeFormatter.ToDot(expr, source);

        result.Should().Contain("ast_method_bend_rank_");
        result.Should().Contain("n0 -> mn2 [");
        result.Should().NotContain("n0 -> mn2 [\r\n\t\t\"tailport\"=");
        result.Should().NotContain("n0 -> mn2 [\n\t\t\"tailport\"=");
    }
}
