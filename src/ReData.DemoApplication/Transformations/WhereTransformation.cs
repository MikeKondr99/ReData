using Pattern.Unions;
using ReData.Query.Common;
using ReData.Query.Core;

namespace ReData.DemoApplication.Transformations;

public class WhereTransformation : ITransformation
{
    public required string Condition { get; set; }

    public Result<QueryBuilder, IEnumerable<ExprError[]>> Apply(QueryBuilder builder)
    {
        return builder.Where(Condition);
    }
}