using System.Text;

namespace ReData.Query.Impl.QueryCompilers;

using Query = Core.Query;

public class SqlServerQueryCompiler : SqlQueryCompiler
{
    protected override void WriteWhere(StringBuilder res, Query query)
    {
        base.WriteWhere(res, query);
        if (query.Limit == 0)
        {
            if (query.Where is null)
            {
                res.Append("WHERE 1 = 0\n");
            }
            else
            {
                res.Append("AND 1 = 0\n");
            }
        }
    }

    protected override void WriteLimitOffset(StringBuilder res, Query query)
    {
        // var a = (query.Limit, query.Offset) switch
        // {
        //     (>0,>0) => res.Append($"OFFSET {query.Offset} ROWS\n FETCH NEXT {query.Limit} ROWS ONLY\n"),
        //     (>0,0) => res.Append($"FETCH NEXT {query.Limit} ROWS ONLY\n"),
        //     (0,>0) => res.Append($"OFFSET {query.Offset} ROWS\n"),
        //     (0,0) => res,
        // }
            
        if (query.Offset.HasValue || query.Limit.HasValue || query.OrderBy?.Count > 0)
        {
            res.Append($"OFFSET {query.Offset ?? 0} ROWS\n");
        }
        
        
        if (query.Limit.HasValue)
        {
            var fetchRows = query.Limit.Value == 0 ? 1 : query.Limit.Value;
            res.Append($"FETCH FIRST {fetchRows} ROWS ONLY\n");
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
            if (query.Limit.HasValue || query.Offset.HasValue)
            {
                res.Append("ORDER BY (SELECT NULL)\n");
            }
            return;
        }
        base.WriteOrderBy(res, query);
    }
}
