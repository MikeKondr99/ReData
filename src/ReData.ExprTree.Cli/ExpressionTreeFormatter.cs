using DotNetGraph;
using DotNetGraph.Compilation;
using DotNetGraph.Core;
using DotNetGraph.Extensions;
using ReData.Query.Lang.Expressions;

namespace ReData.ExprTree.Cli;

public static class ExpressionTreeFormatter
{
    public static string ToDot(Expr expr, string sourceText, SqlTraceResult? sqlTrace = null)
    {
        var graph = new DotGraph()
            .WithIdentifier("ExpressionTree")
            .Directed()
            .WithAttribute("splines", "ortho");

        var nodes = new Dictionary<Expr, string>(ReferenceEqualityComparer.Instance);
        var index = 0;
        var rootNodeId = BuildDotGraph(expr, graph, nodes, ref index);
        AddSourceNode(sourceText, graph, rootNodeId);
        AddSqlLayer(sqlTrace, graph, nodes, ref index);

        using var writer = new StringWriter();
        var context = new CompilationContext(writer, new CompilationOptions());
        graph.CompileAsync(context).GetAwaiter().GetResult();
        return writer.ToString();
    }

    private static string BuildDotGraph(
        Expr expr,
        DotGraph graph,
        IDictionary<Expr, string> nodes,
        ref int index)
    {
        if (nodes.TryGetValue(expr, out var existingId))
        {
            return existingId;
        }

        var currentId = $"n{index++}";
        nodes[expr] = currentId;

        graph.Elements.Add(new DotNode()
            .WithIdentifier(currentId)
            .WithLabel(GetLabel(expr)));

        if (expr is not FuncExpr func)
        {
            return currentId;
        }

        for (var i = 0; i < func.Arguments.Count; i++)
        {
            var childId = BuildDotGraph(func.Arguments[i], graph, nodes, ref index);
            if (func.Kind == FuncExprKind.Method && i == 0)
            {
                var bendNodeId = $"mn{index++}";
                graph.Elements.Add(new DotNode()
                    .WithIdentifier(bendNodeId)
                    .WithAttribute("shape", "point")
                    .WithAttribute("width", "0.01")
                    .WithAttribute("height", "0.01")
                    .WithAttribute("style", "invis")
                    .WithAttribute("group", childId));

                var bendRank = new DotSubgraph()
                    .WithIdentifier($"ast_method_bend_rank_{bendNodeId}", false)
                    .WithAttribute("rank", "same");
                bendRank.Add(new DotNode().WithIdentifier(currentId));
                bendRank.Add(new DotNode().WithIdentifier(bendNodeId));
                graph.Add(bendRank);

                graph.Elements.Add(new DotEdge()
                    .From(currentId)
                    .To(bendNodeId)
                    .WithAttribute("arrowhead", "none")
                    .WithAttribute("constraint", "false"));

                graph.Elements.Add(new DotEdge()
                    .From(bendNodeId)
                    .To(childId)
                    .WithAttribute("tailport", "s"));
                continue;
            }

            if (func.Kind == FuncExprKind.Binary && func.Arguments.Count == 2)
            {
                var bendNodeId = $"bn{index++}";
                graph.Elements.Add(new DotNode()
                    .WithIdentifier(bendNodeId)
                    .WithAttribute("shape", "point")
                    .WithAttribute("width", "0.01")
                    .WithAttribute("height", "0.01")
                    .WithAttribute("style", "invis")
                    .WithAttribute("group", childId));

                var bendRank = new DotSubgraph()
                    .WithIdentifier($"ast_bend_rank_{bendNodeId}", false)
                    .WithAttribute("rank", "same");
                bendRank.Add(new DotNode().WithIdentifier(currentId));
                bendRank.Add(new DotNode().WithIdentifier(bendNodeId));
                graph.Add(bendRank);

                var tailPort = i == 0 ? "w" : "e";

                graph.Elements.Add(new DotEdge()
                    .From(currentId)
                    .To(bendNodeId)
                    .WithAttribute("tailport", tailPort)
                    .WithAttribute("arrowhead", "none")
                    .WithAttribute("constraint", "false"));

                graph.Elements.Add(new DotEdge()
                    .From(bendNodeId)
                    .To(childId));
                continue;
            }

            if (func.Arguments.Count == 1)
            {
                var bendNodeId = $"un{index++}";
                graph.Elements.Add(new DotNode()
                    .WithIdentifier(bendNodeId)
                    .WithAttribute("shape", "point")
                    .WithAttribute("width", "0.01")
                    .WithAttribute("height", "0.01")
                    .WithAttribute("style", "invis")
                    .WithAttribute("group", childId));

                var bendRank = new DotSubgraph()
                    .WithIdentifier($"ast_unary_bend_rank_{bendNodeId}", false)
                    .WithAttribute("rank", "same");
                bendRank.Add(new DotNode().WithIdentifier(currentId));
                bendRank.Add(new DotNode().WithIdentifier(bendNodeId));
                graph.Add(bendRank);

                graph.Elements.Add(new DotEdge()
                    .From(currentId)
                    .To(bendNodeId)
                    .WithAttribute("arrowhead", "none")
                    .WithAttribute("constraint", "false"));

                graph.Elements.Add(new DotEdge()
                    .From(bendNodeId)
                    .To(childId));
                continue;
            }

            var directEdge = new DotEdge()
                .From(currentId)
                .To(childId);

            if (func.Arguments.Count > 1)
            {
                if (func.Kind != FuncExprKind.Default && func.Kind != FuncExprKind.Method)
                {
                    var midpoint = (func.Arguments.Count - 1) / 2.0;
                    var tailPort = i <= midpoint ? "w" : "e";
                    directEdge = directEdge.WithAttribute("tailport", tailPort);
                }
            }

            graph.Elements.Add(directEdge);
        }

        return currentId;
    }

    private static string GetLabel(Expr expr)
    {
        return expr switch
        {
            FuncExpr func => func.Name,
            NameExpr name => name.Value,
            _ => expr.ToString(),
        };
    }

    private static void AddSourceNode(string sourceText, DotGraph graph, string rootNodeId)
    {
        var sourceId = "source";
        var sourceLabel = sourceText
            .Replace("\r", string.Empty)
            .Replace("\\r\\n", "\n")
            .Replace("\\n", "\n");
        graph.Elements.Add(new DotNode()
            .WithIdentifier(sourceId)
            .WithLabel(sourceLabel)
            .WithAttribute("shape", "box"));
        graph.Elements.Add(new DotEdge()
            .From(sourceId)
            .To(rootNodeId));
    }

    private static void AddSqlLayer(
        SqlTraceResult? sqlTrace,
        DotGraph graph,
        IReadOnlyDictionary<Expr, string> exprNodeIds,
        ref int index)
    {
        if (sqlTrace is null || sqlTrace.Tokens.Count == 0)
        {
            return;
        }

        var tokenNodes = new List<string>(sqlTrace.Tokens.Count);
        for (var i = 0; i < sqlTrace.Tokens.Count; i++)
        {
            var tokenNodeId = $"q{index++}";
            tokenNodes.Add(tokenNodeId);
            var tokenLabel = EscapeSqlTokenLabel(sqlTrace.Tokens[i].Token);
            graph.Elements.Add(new DotNode()
                .WithIdentifier(tokenNodeId)
                .WithLabel(tokenLabel)
                .WithAttribute("shape", "plaintext")
                .WithAttribute("group", tokenNodeId));
        }

        var subgraph = new DotSubgraph()
            .WithIdentifier("sql_tokens", false)
            .WithAttribute("rank", "sink");
        foreach (var nodeId in tokenNodes)
        {
            subgraph.Add(new DotNode().WithIdentifier(nodeId));
        }

        graph.Add(subgraph);

        for (var i = 0; i < tokenNodes.Count - 1; i++)
        {
            graph.Elements.Add(new DotEdge()
                .From(tokenNodes[i])
                .To(tokenNodes[i + 1])
                .WithStyle("invis")
                .WithAttribute("weight", "100"));
        }

        var ownerToTokenIndices = new Dictionary<Expr, List<int>>(ReferenceEqualityComparer.Instance);
        for (var i = 0; i < sqlTrace.Tokens.Count; i++)
        {
            var owner = sqlTrace.Tokens[i].SourceExpr;
            if (owner is null)
            {
                continue;
            }

            if (!ownerToTokenIndices.TryGetValue(owner, out var list))
            {
                list = [];
                ownerToTokenIndices[owner] = list;
            }

            list.Add(i);
        }

        foreach (var pair in exprNodeIds)
        {
            var expr = pair.Key;
            var exprNodeId = pair.Value;
            if (!ownerToTokenIndices.TryGetValue(expr, out var tokenIndices) || tokenIndices.Count == 0)
            {
                continue;
            }

            var primaryTokenIndices = ResolvePrimaryTokenIndices(expr, tokenIndices, sqlTrace.Tokens);
            foreach (var tokenIndex in primaryTokenIndices)
            {
                graph.Elements.Add(new DotEdge()
                    .From(exprNodeId)
                    .To(tokenNodes[tokenIndex])
                    .WithStyle("dashed")
                    .WithColor("steelblue4")
                    .WithAttribute("headport", "n")
                    .WithAttribute("weight", "20"));
            }

            var primaryTokenSet = primaryTokenIndices.ToHashSet();

            foreach (var tokenIndex in tokenIndices)
            {
                if (primaryTokenSet.Contains(tokenIndex))
                {
                    continue;
                }

                graph.Elements.Add(new DotEdge()
                    .From(exprNodeId)
                    .To(tokenNodes[tokenIndex])
                    .WithStyle("dashed")
                    .WithColor("gray60")
                    .WithAttribute("headport", "n")
                    .WithAttribute("constraint", "false"));
            }
        }
    }

    private static IReadOnlyList<int> ResolvePrimaryTokenIndices(
        Expr expr,
        IReadOnlyList<int> tokenIndices,
        IReadOnlyList<SqlTokenTrace> tokens)
    {
        var exprLabel = NormalizeForMatch(GetLabel(expr));
        var matchingIndices = tokenIndices
            .Where(i => string.Equals(NormalizeForMatch(tokens[i].Token), exprLabel, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (matchingIndices.Length > 0)
        {
            return matchingIndices;
        }

        if (expr is FuncExpr { Kind: FuncExprKind.Binary })
        {
            return [tokenIndices[tokenIndices.Count / 2]];
        }

        if (expr is FuncExpr)
        {
            var firstToken = NormalizeForMatch(tokens[tokenIndices[0]].Token);
            var repeatedStartTokens = tokenIndices
                .Where(i => string.Equals(NormalizeForMatch(tokens[i].Token), firstToken, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (repeatedStartTokens.Length > 1)
            {
                return repeatedStartTokens;
            }

            return [tokenIndices[0]];
        }

        return [tokenIndices[0]];
    }

    private static string NormalizeForMatch(string token)
    {
        var normalized = token.Trim();
        if (normalized.Length >= 2 && normalized[0] == '"' && normalized[^1] == '"')
        {
            normalized = normalized[1..^1];
        }

        normalized = normalized.Trim('(', ')', '[', ']', '{', '}', ',', ';');

        return normalized;
    }

    private static string EscapeSqlTokenLabel(string token)
    {
        return token.Replace("\\", "\\\\");
    }
}
