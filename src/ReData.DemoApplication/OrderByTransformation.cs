using Pattern.Unions;
using ReData.Query.Common;
using ReData.Query.Core;

namespace ReData.DemoApplication;

public class OrderByTransformation : ITransformation
{
    public required OrderItem[] Items { get; init; }

    public Result<QueryBuilder, IEnumerable<ExprError?>> Apply(QueryBuilder builder)
    {
        return builder.OrderBy(Items.Select(i => (i.Expression, i.Descending ? Query.Core.Types.OrderItem.Type.Desc : Query.Core.Types.OrderItem.Type.Asc)).ToArray());
    }
}

public class LimitTransformation : ITransformation
{
    public uint Limit { get; init; }
    
    public uint Offset   { get; init; }
    public Result<QueryBuilder, IEnumerable<ExprError?>> Apply(QueryBuilder builder)
    {
        return Result.Ok(builder.Take(Limit).Skip(Offset));
    }
}

public record struct OrderItem
{
    public required string Expression { get; init; }
    public required bool Descending { get; init; }
}