using System.Text;
using ReData.Common;
using ReData.Query.Core.Components;
using ReData.Query.Core.Template;
using ReData.Query.Core.Types;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Impl.QueryCompilers;

using Query = Core.Query;

public abstract class SqlQueryCompiler : IQueryCompiler
{
    public required IExpressionCompiler ExpressionCompiler { protected get; init; }

    public string Compile(Query query)
    {
        using var span = Tracing.Source.StartActivity("query compilation");
        StringBuilder res = new StringBuilder();
        WriteQuery(res,query, true);
        res.TrimEnd();
        var result = res.ToString();
        span?.SetTag("result", result);
        return result;
    }

    protected virtual void WriteQuery(StringBuilder res, Query query, bool withSubqueries = false)
    {
        if (withSubqueries)
        {
            WriteSubqueries(res, query);
        }
        WriteSelect(res, query);
        WriteWhere(res, query);
        WriteGroupBy(res, query);
        // Having
        WriteOrderBy(res, query);
        WriteLimitOffset(res, query);
    }

    protected virtual IEnumerable<Query> FindSubqueries(Query query)
    {
        if (query.From is Query from)
        {
            foreach (var s in FindSubqueries(from))
            {
                yield return s;
            }
            yield return from;
        }
    } 
    
    protected virtual void WriteSubqueries(StringBuilder res, Query query)
    {
        var subs = FindSubqueries(query).ToArray();
        if (subs.Length > 0)
        {
            res.Append("WITH\n");
            foreach (var sub in subs)
            {
                WriteExpression(res, query, sub.Name);
                res.Append(" AS (\n");
                WriteQuery(res, sub);
                res.Append("),\n");
            }
            res.Length -= 2;
            res.Append('\n');
        }
    }

    protected virtual void WriteLimitOffset(StringBuilder res, Query query)
    {
        if (query.Limit > 0)
        {
            res.Append($"LIMIT {query.Limit}\n");
        }
        
        if (query.Offset > 0)
        {
            res.Append($"OFFSET {query.Offset}\n");
        }
    }

    protected virtual void WriteSelect(StringBuilder res, Query query)
    {
        if (query.Select?.Count is 0 or null)
        {
            res.Append("SELECT * ");
        }
        else
        {
            res.Append("SELECT\n");
            WriteSelectTransformations(res, query);
        }

        WriteFrom(res, query);
    }

    protected virtual void WriteFrom(StringBuilder res, Query query)
    {
        if (query.From.Name is not null)
        {
            res.Append($"FROM ");
            ExpressionCompiler.Compile(res, query.From.Name);
        }
        res.Append('\n');
    }
    

    protected virtual void WriteSelectTransformations(StringBuilder res, Query query)
    {
        int last = query.Select!.Count - 1;
        for (var i = 0; i < query.Select.Count; i++)
        {
            var field = query.Select[i];

            res.Append("    ");
            ExpressionCompiler.Compile(res, field.ResolvedExpr);
            res.Append(" AS ");
            WriteExpression(res, query, field.Column);
            
            if (i != last)
            {
                res.Append(", ");
            }
            res.Append('\n');
        }
    }

    protected virtual void WriteExpression(StringBuilder res, Query query, IResolvedTemplate node)
    {
        ExpressionCompiler.Compile(res, node);
    }

    protected virtual void WriteWhere(StringBuilder res, Query query)
    {
        if (query.Where is null)
        {
            return;
        }

        var init = "WHERE ";
        foreach (var filter in query.Where)
        {
            res.Append(init);
            init = "AND ";
            WriteExpression(res, query, filter);
            res.Append('\n');
        }
    }
    
    protected virtual void WriteOrderBy(StringBuilder res, Query query)
    {
        if (query.OrderBy?.Count is 0 or null)
        {
            return;
        }

        int last = query.OrderBy.Count - 1;
        res.Append("ORDER BY ");
        for (var i = 0; i < query.OrderBy.Count; i++)
        {
            var order = query.OrderBy[i];
            WriteExpression(res, query, order.ResolvedExpr);
            res.Append(' ');
            res.Append(order.Direction switch
            {
                OrderItem.Type.Desc => "DESC",
                OrderItem.Type.Asc => "ASC",
                var unknown => throw new Exception($"Unknown enum value {unknown} of type Query.Order.Type")
            });

            if (i != last)
            {
                res.Append(", ");
            }
        }
        res.Append('\n');
    }
    
    protected virtual void WriteGroupBy(StringBuilder res, Query query)
    {
        if (query.GroupBy?.Count is 0 or null)
        {
            return;
        }

        int last = query.GroupBy.Count - 1;
        res.Append("GROUP BY ");
        for (var i = 0; i < query.GroupBy.Count; i++)
        {
            var group = query.GroupBy[i];
            WriteExpression(res, query, group);
            if (i != last)
            {
                res.Append(", ");
            }
        }
        res.Append('\n');
    }

}