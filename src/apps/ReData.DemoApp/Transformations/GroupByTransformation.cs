using Pattern.Unions;
using ReData.Query.Common;
using ReData.Query.Core;

namespace ReData.DemoApp.Transformations;

/// <summary>
/// $type = "groupBy"
/// </summary>
public class GroupByTransformation : Transformation
{
    public required SelectItem[] Groups { get; init; }
    public required SelectItem[] Items { get; init; }

    public override Result<QueryBuilder, IEnumerable<IReadOnlyList<ExprError>>> Apply(QueryBuilder builder)
    {
        var select = Groups.Concat(Items).ToDictionary(a => a.Field, a => a.Expression);
        var groups = Groups.Select(a => a.Expression);
        return builder.GroupBy(groups.ToArray(), select);
    }
}