using Pattern.Unions;
using ReData.Query.Common;
using ReData.Query.Core;

namespace ReData.DemoApplication;

public class WhereTransformation : ITransformation
{
    public required string Condition { get; set; }

    public Result<QueryBuilder, IEnumerable<ExprError?>> Apply(QueryBuilder builder, IServiceProvider services)
    {
        return builder.Where(Condition);
    }
}