using Pattern.Unions;
using ReData.Query.Common;
using ReData.Query.Core;

namespace ReData.DemoApp.Transformations;

/// <summary>
/// $type = "where"
/// </summary>
public class WhereTransformation : Transformation
{
    public required string Condition { get; set; }

    public override Result<QueryBuilder, IEnumerable<IReadOnlyList<ExprError>>> Apply(QueryBuilder builder)
    {
        return builder.Where(Condition);
    }
}