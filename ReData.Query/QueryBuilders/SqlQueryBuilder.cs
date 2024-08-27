using System.Text;
using ReData.Query.Lang.Expressions;

namespace ReData.Query;

public abstract class SqlQueryBuilder : IQueryBuilder
{
    public required IExpressionBuilder ExpressionBuilder { private get; init; }
    
    public virtual string Build(Query query)
    {
        StringBuilder res = new StringBuilder();
        WriteQuery(res,query);
        res.TrimEnd();
        return res.ToString();
    }

    protected virtual void WriteQuery(StringBuilder res, Query query)
    {
        WriteCommonTables(res, query);
        WriteSelect(res, query);
        WriteWhere(res, query);
        // GroupBy
        // Having
        WriteOrdeBy(res, query);
        WriteLimitOffset(res, query);
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

    protected void WriteCommonTables(StringBuilder res, Query query)
    {
        if (query.CommonTables is null) return;
        
        int last = query.CommonTables.Count() - 1;
        res.Append("WITH ");
        var _ = query.CommonTables.Select((Query cte,int i) =>
        {
            WriteField(res,$"CTE{i + 1}");
            res.Append($" AS (\n");
            WriteQuery(res,cte);
            return res.Append(i != last ? "),\n" : ")\n");
        });
    }
    
    protected void WriteSelect(StringBuilder res, Query query)
    {
        res.Append("SELECT ");
        if (query.Select.Count == 0)
        {
            res.Append('*');
        }
        else
        {
            WriteSelectTransformations(res, query);
        }
        res.Append($" FROM ");
        WriteField(res, query.Table);
        res.Append('\n');
    }

    protected void WriteSelectTransformations(StringBuilder res, Query query)
    {
        int last = query.Select.Count - 1;

        for (var i = 0; i < query.Select.Count; i++)
        {
            var field = query.Select[i];

            if (field.Value is not null)
            {
                ExpressionBuilder.Write(res,field.Value, query.Fields);
                res.Append(" AS ");
            }
            
            WriteField(res,field.Name);
            if (i != last)
            {
                res.Append(", ");
            }
        }
    }

    protected virtual void WriteExpression(StringBuilder res, IExpr expr, IReadOnlyDictionary<string, ExprType> fields)
    {
        ExpressionBuilder.Write(res, expr, fields);
    }

    protected virtual void WriteWhere(StringBuilder res, Query query)
    {
        foreach (var filter in query.Where)
        {
            res.Append("WHERE ");
            WriteExpression(res, filter, query.Fields);
            res.Append('\n');
        }
    }
    
    protected virtual void WriteOrdeBy(StringBuilder res, Query query)
    {
        if (query.OrderBy.Count == 0)
        {
            return;
        }
        
        int last = query.Select.Count - 1;
        res.Append("ORDER BY ");
        for (var i = 0; i < query.OrderBy.Count; i++)
        {
            var order = query.OrderBy[i];
            WriteExpression(res, order.Item1, query.Fields);
            res.Append(order.Item2 == Query.Order.Desc ? " DESC" : " ASC");

            if (i != last)
            {
                res.Append(", ");
            }

        }
        res.Append('\n');
    }

    protected virtual void WriteField(StringBuilder res, string field)
    {
        res.Append('"');
        res.Append(field);
        res.Append('"');
    }
}