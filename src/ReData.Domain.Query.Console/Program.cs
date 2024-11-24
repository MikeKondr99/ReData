using DotNetGraph.Compilation;
using ReData.Query.Lang.Expressions;
using ReData.Query.Visitors;


while (true)
{
    Console.Write("Expr: ");
    var line = Console.ReadLine();
    if (line?.Trim() is "q" or null)
    {
        break;
    }

    var expr = Expr.Parse(line);

    var graph = GraphVisitor.GetGraph(expr);

    await using var writer = new StringWriter();
    var context = new CompilationContext(writer, new CompilationOptions());
    await graph.CompileAsync(context);

    var result = writer.GetStringBuilder().ToString();

    string cmd = $"/C code - graph.dot";
    System.Diagnostics.Process.Start("CMD.exe",cmd);
    
    File.WriteAllText("graph.dot", result);
}



