using DotNetGraph.Core;
using DotNetGraph.Extensions;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Visitors;

public sealed class GraphVisitor : ExprVisitor<DotNode>
{
    
    private GraphVisitor(DotGraph graph)
    {
        _graph = graph;
    }

    private DotGraph _graph;
    private int counter = 0;

    public string Id => (counter++).ToString();

    public static DotGraph GetGraph(IExpr expr)
    {
        var graph = new DotGraph().WithIdentifier("graph").Directed();
        var visitor = new GraphVisitor(graph);
        var node = visitor.Visit(expr);
        return graph;
    }
    
    public override DotNode Visit(StringLiteral expr)
    {
        var node = new DotNode()
            .WithLabel($"\"{expr.Value}\"")
            .WithIdentifier(Id)
            .WithShape(DotNodeShape.Box);
        _graph.Add(node);
        return node;
    }

    public override DotNode Visit(NumberLiteral expr)
    {
        var node = new DotNode()
            .WithLabel($"{expr.Value:0.0}")
            .WithIdentifier(Id)
            .WithShape(DotNodeShape.Box);
        _graph.Add(node);
        return node;
    }

    public override DotNode Visit(IntegerLiteral expr)
    {
        var node = new DotNode()
            .WithLabel($"{expr.Value}")
            .WithIdentifier(Id)
            .WithShape(DotNodeShape.Box);
        _graph.Add(node);
        return node;
    }

    public override DotNode Visit(BooleanLiteral expr)
    {
        var node = new DotNode()
            .WithLabel($"{expr.Value}")
            .WithIdentifier(Id)
            .WithShape(DotNodeShape.Box);
        _graph.Add(node);
        return node;
    }
    
    public override DotNode Visit(NameExpr expr)
    {
        var node = new DotNode()
            .WithLabel($"[{expr.Value}]")
            .WithIdentifier(Id)
            .WithShape(DotNodeShape.Box);
        _graph.Add(node);
        return node;
    }

    public override DotNode Visit(NullLiteral expr)
    {
        var node = new DotNode()
            .WithLabel($"null")
            .WithIdentifier(Id)
            .WithShape(DotNodeShape.Box);
        _graph.Add(node);
        return node;
    }

    public override DotNode Visit(FuncExpr expr)
    {
        var func = new DotNode()
            .WithLabel(expr.Name)
            .WithIdentifier(Id);
        
        foreach (var a in expr.Arguments)
        {
            var arg = Visit(a);
            var edge = new DotEdge().From(func).To(arg);
            _graph.Add(edge);
        }
        _graph.Add(func);
        return func;
    }
    
}