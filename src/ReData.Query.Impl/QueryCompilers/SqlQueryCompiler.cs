using System.Diagnostics;
using System.Text;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Impl.QueryBuilders;

public abstract class SqlQueryCompiler : IQueryCompiler
{
    public required IExpressionBuilder ExpressionBuilder { protected get; init; }
    
    public required IFunctionStorage FunctionsStorage { protected get; init; }

    public virtual string Compile(Query query)
    {
        StringBuilder res = new StringBuilder();
        WriteQuery(res,query, true);
        res.TrimEnd();
        return res.ToString();
    }

    protected virtual void WriteQuery(StringBuilder res, Query query, bool withSubqueries = false)
    {
        if (withSubqueries)
        {
            WriteSubqueries(res, query);
        }
        WriteSelect(res, query);
        WriteWhere(res, query);
        // GroupBy
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
                WriteExpression(res, query, new NameExpr(sub.Name));
                res.Append(" AS (\n");
                WriteQuery(res, sub);
                res.Append("),\n");
            }
            res.Length-=2;
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
            ExpressionBuilder.Write(res,new NameExpr(query.From.Name),query.From.Fields(FunctionsStorage));
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
            if (!(field.Expr is NameExpr ne && ne.Value == field.Name))
            {
                ExpressionBuilder.Write(res,field.Expr, query.From.Fields(FunctionsStorage));
                res.Append(" AS ");
            }
            
            WriteExpression(res, query, new NameExpr(field.Name));
            
            if (i != last)
            {
                res.Append(", ");
            }
            res.Append('\n');
        }
    }

    protected virtual void WriteExpression(StringBuilder res, Query query, IExpr expr)
    {
        ExpressionBuilder.Write(res, expr, query.From.Fields(FunctionsStorage));
    }

    protected virtual void WriteWhere(StringBuilder res, Query query)
    {
        if (query.Where is null) return;
        foreach (var filter in query.Where)
        {
            res.Append("WHERE ");
            WriteExpression(res, query, filter);
            res.Append('\n');
        }
    }
    
    protected virtual void WriteOrderBy(StringBuilder res, Query query)
    {
        if (query.OrderBy?.Count is 0 or null) return;
        
        int last = query.OrderBy.Count - 1;
        res.Append("ORDER BY ");
        for (var i = 0; i < query.OrderBy.Count; i++)
        {
            var order = query.OrderBy[i];
            WriteExpression(res, query, order.Expr);
            res.Append(order.Direction switch
            {
                Query.Order.Type.Desc => "DESC",
                Query.Order.Type.Asc => "ASC",
                var unknown => throw new Exception($"Unknown enum value {unknown} of type Query.Order.Type")
            });

            if (i != last)
            {
                res.Append(", ");
            }
        }
        res.Append('\n');
    }

}