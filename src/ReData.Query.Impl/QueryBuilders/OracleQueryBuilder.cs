using System.Text;
using ReData.Query.Lang.Expressions;
using ReData.Query.Visitors;

namespace ReData.Query.Impl.QueryBuilders;

public class OracleQueryBuilder : SqlQueryBuilder
{
    protected override void WriteLimitOffset(StringBuilder res, Query query)
    {
        //OFFSET 50 ROWS FETCH NEXT 100 ROWS ONLY;
        if (query.Offset > 0 || query.Limit > 0)
        {
            res.Append($"OFFSET {query.Offset} ROWS\n");
        }
        if (query.Limit > 0)
        {
            res.Append($"FETCH NEXT {query.Limit} ROWS ONLY\n");
        }
    }

    protected override void WriteFrom(StringBuilder res, Query query)
    {
        if (query.From.Name is not null)
        {
            res.Append($"FROM ");
            ExpressionBuilder.Write(res,new NameExpr(query.From.Name),query.From.Fields(FunctionsStorage));
        }
        else
        {
            res.Append($"FROM DUAL");
        }
        res.Append('\n');
    }

    protected override void WriteOrderBy(StringBuilder res, Query query)
    {
        if (query.OrderBy?.Count is 0 or null)
        {
            if (query.Limit > 0)
            {
                res.Append("ORDER BY ROWNUM\n");
            }
            return;
        }
        base.WriteOrderBy(res, query);
    }
}