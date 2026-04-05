using System.Text;

namespace ReData.Query.Impl.QueryCompilers;

public class MySqlQueryCompiler : SqlQueryCompiler
{
    protected override void WriteLimitOffset(StringBuilder res, Core.Query query)
    {
        if (query.Limit > 0 && query.Offset > 0)
        {
            res.Append($"LIMIT {query.Limit}\n OFFSET {query.Offset}\n");
            return;
        }

        if (query.Limit > 0)
        {
            res.Append($"LIMIT {query.Limit}\n");
            return;
        }

        if (query.Offset > 0)
        {
            res.Append($"LIMIT 18446744073709551615\n OFFSET {query.Offset}\n");
        }
    }
}
