using System.Text;

namespace ReData.Query.Impl.QueryCompilers;

public class MySqlQueryCompiler : SqlQueryCompiler
{
    protected override void WriteLimitOffset(StringBuilder res, Core.Query query)
    {
        var _ = (query.Limit, query.Offset) switch
        {
            (> 0, > 0) => res.Append($"LIMIT {query.Limit}\n OFFSET {query.Offset}\n"),
            (> 0, _) => res.Append($"LIMIT {query.Limit}\n"),
            (_, > 0) => res.Append($"LIMIT 18446744073709551615\n OFFSET {query.Offset}\n"),
            (_, _) => res,
        };
    }
}