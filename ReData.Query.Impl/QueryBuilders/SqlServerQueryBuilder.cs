using System.Text;

namespace ReData.Query.Impl.QueryBuilders;

public class SqlServerQueryBuilder : SqlQueryBuilder
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

    protected override void WriteSelect(StringBuilder res, Query query)
    {
        var top = (query.Limit == 0 && query.OrderBy?.Count is >0 ) ? "TOP(99.99999999999) PERCENT" : "";
        if (query.Select?.Count is 0 or null)
        {
            res.Append($"SELECT {top} * ");
        }
        else
        {
            res.Append($"SELECT {top}\n");
            WriteSelectTransformations(res, query);
        }

        WriteFrom(res, query);
    }

    protected override void WriteOrderBy(StringBuilder res, Query query)
    {
        if (query.OrderBy?.Count is 0 or null)
        {
            // MsSql требует наличия ORDER BY для лимита
            if (query.Limit > 0)
            {
                res.Append("ORDER BY (SELECT NULL)\n");
            }
            return;
        }
        base.WriteOrderBy(res, query);
    }
}