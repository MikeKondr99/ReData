using Pattern.Unions;
using ReData.Query.Common;
using ReData.Query.Core;

namespace ReData.DemoApp.Transformations;

public class LimitOffsetTransformation : ITransformation
{
    public uint? Limit { get; set; }
    public uint? Offset { get; set; }

    public Result<QueryBuilder, IEnumerable<IReadOnlyList<ExprError>>> Apply(QueryBuilder builder)
    {
        if (Offset.HasValue)
        {
            builder = builder.Skip(Offset.Value);
        }
        if (Limit.HasValue)
        {
            builder = builder.Take(Limit.Value);
        }
        return builder;
    }
}