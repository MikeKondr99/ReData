using Pattern.Unions;
using ReData.Query.Core;

namespace ReData.DemoApplication;

public class LimitOffsetTransformation : ITransformation
{
    public uint? Take { get; set; }
    public uint? Skip { get; set; }

    public Result<QueryBuilder, IEnumerable<ExprError?>> Apply(QueryBuilder builder)
    {
        if (Skip.HasValue)
        {
            builder = builder.Skip(Skip.Value);
        }
        if (Take.HasValue)
        {
            builder = builder.Take(Take.Value);
        }
        return builder;
    }
}