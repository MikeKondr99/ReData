using Pattern.Unions;
using ReData.Query.Common;
using ReData.Query.Core;

namespace ReData.DemoApplication.Transformations;

public class SelectTransformation : ITransformation
{
    public required SelectItem[] Items { get; init; }

    public Result<QueryBuilder, IEnumerable<ExprError?>> Apply(QueryBuilder builder)
    {
        return builder.Select(Items.ToDictionary(a => a.Field, a => a.Expression));
    }
}

public sealed record SelectItem
{
    public required string Field { get; init; }
    public required string Expression { get; init; }
}