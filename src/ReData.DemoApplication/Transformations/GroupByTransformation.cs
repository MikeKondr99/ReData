using Pattern.Unions;
using ReData.Query.Common;
using ReData.Query.Core;

namespace ReData.DemoApplication;

public class GroupByTransformation : ITransformation
{
    public required SelectItem[] Groups { get; init; }
    public required SelectItem[] Items { get; init; }

    public Result<QueryBuilder, IEnumerable<ExprError?>> Apply(QueryBuilder builder, IServiceProvider services)
    {
        var select = Groups.Concat(Items).ToDictionary(a => a.Field, a => a.Expression);
        var groups = Groups.Select(a => a.Expression);
        return builder.GroupBy(groups.ToArray(), select);
    }
}