using System.Text;

namespace ReData.Query.Impl.QueryCompilers;

using Query = Core.Query;

public class SqlServerQueryCompiler : SqlQueryCompiler
{
    protected override void WriteLimitOffset(StringBuilder res, Query query)
    {
        // var a = (query.Limit, query.Offset) switch
        // {
        //     (>0,>0) => res.Append($"OFFSET {query.Offset} ROWS\n FETCH NEXT {query.Limit} ROWS ONLY\n"),
        //     (>0,0) => res.Append($"FETCH NEXT {query.Limit} ROWS ONLY\n"),
        //     (0,>0) => res.Append($"OFFSET {query.Offset} ROWS\n"),
        //     (0,0) => res,
        // }

        if (query.Offset > 0 || query.Limit > 0 || query.OrderBy?.Count > 0)
        {
            res.Append($"OFFSET {query.Offset} ROWS\n");
        }


        if (query.Limit > 0)
        {
            res.Append($"FETCH FIRST {query.Limit} ROWS ONLY\n");
        }
        else if (query is { OrderBy.Count: > 0 })
        {
            res.Append($"FETCH NEXT 100000000000000 ROWS ONLY\n");
        }
    }

    protected override void WriteOrderBy(StringBuilder res, Query query)
    {
        if (query.OrderBy?.Count is 0 or null)
        {
            // MsSql требует наличия ORDER BY для лимита
            if (query.Limit > 0 || query.Offset > 0)
            {
                res.Append("ORDER BY (SELECT NULL)\n");
            }
            return;
        }
        base.WriteOrderBy(res, query);
    }
}
