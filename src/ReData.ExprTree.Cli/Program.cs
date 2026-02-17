using ReData.ExprTree.Cli;
using ReData.Query.Lang.Expressions;

Console.WriteLine("Expression Tree CLI");
Console.WriteLine("Input mode:");
Console.WriteLine("- finish expression with line: ;;");
Console.WriteLine("- exit with line: :q");

while (true)
{
    var input = ReadExpressionBlock();
    if (input is null)
    {
        return;
    }

    input = input.TrimStart('\uFEFF');

    var parseResult = Expr.Parse(input);
    if (parseResult.IsError(out var error))
    {
        Console.WriteLine($"Parse error: {error.Message}");
        Console.WriteLine($"Span: {error.Span.StartRow}:{error.Span.StartColumn} - {error.Span.EndRow}:{error.Span.EndColumn}");
        Console.WriteLine();
        continue;
    }

    var expr = parseResult.Unwrap();
    SqlTraceResult? sqlTrace = null;
    try
    {
        sqlTrace = PostgresExpressionSqlResolver.Resolve(expr);
    }
    catch (Exception exception)
    {
        Console.WriteLine($"PostgreSQL resolve warning: {exception.Message}");
    }

    if (sqlTrace is not null)
    {
        Console.WriteLine($"PostgreSQL SQL: {sqlTrace.Sql}");
    }

    var dot = ExpressionTreeFormatter.ToDot(expr, input, sqlTrace);
    var outputPath = Path.Combine(Path.GetTempPath(), $"expr-tree-{DateTime.UtcNow:yyyyMMdd-HHmmssfff}.svg");

    try
    {
        GraphvizRenderer.RenderSvg(dot, outputPath);
        Console.WriteLine($"Rendered: {outputPath}");

        try
        {
            GraphvizRenderer.OpenFile(outputPath);
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Open warning: {exception.Message}");
        }
    }
    catch (Exception exception)
    {
        Console.WriteLine($"Render error: {exception.Message}");
    }

    Console.WriteLine();
}

static string? ReadExpressionBlock()
{
    Console.WriteLine();
    Console.WriteLine("Enter expression (end with ;;):");
    var lines = new List<string>();

    while (true)
    {
        Console.Write(lines.Count == 0 ? "> " : "| ");
        var line = Console.ReadLine();
        if (line is null)
        {
            return null;
        }

        if (lines.Count == 0 && string.Equals(line.Trim(), ":q", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (line.Trim() == ";;")
        {
            break;
        }

        lines.Add(line);
    }

    if (lines.Count == 0)
    {
        return string.Empty;
    }

    return string.Join(Environment.NewLine, lines);
}
