using System.Text;

namespace ReData.Query.Impl.QueryCompilers;

public class MySqlQueryCompiler : SqlQueryCompiler
{
    protected override void WriteLimitOffset(StringBuilder res, Core.Query query)
    {
        if (query.Limit.HasValue && query.Offset is > 0)
        {
            res.Append($"LIMIT {query.Limit.Value}\n OFFSET {query.Offset.Value}\n");
            return;
        }

        if (query.Limit.HasValue)
        {
            res.Append($"LIMIT {query.Limit.Value}\n");
            return;
        }

        if (query.Offset is > 0)
        {
            res.Append($"LIMIT 18446744073709551615\n OFFSET {query.Offset.Value}\n");
        }
    }
}
