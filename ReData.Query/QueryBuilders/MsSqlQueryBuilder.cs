using System.Text;

namespace ReData.Query;

public class MsSqlQueryBuilder : SqlQueryBuilder
{
    protected override void WriteLimitOffset(StringBuilder res, Query query)
    {
        //OFFSET 50 ROWS FETCH NEXT 100 ROWS ONLY;
        if (query.Offset > 0)
        {
            res.Append($"OFFSET {query.Offset} ROWS\n");
        }
        if (query.Limit > 0)
        {
            res.Append($"FETCH NEXT {query.Limit} ROWS ONLY\n");
        }
    }
}