using ReData.Query.Visitors;

namespace ReData.DemoApplication;

public class SelectTransformation : ITransformation
{
    public required SelectItem[] Items { get; init; }

    public QueryBuilder Apply(QueryBuilder builder)
    {
        
        return builder.Select(Items.ToDictionary(a => a.Field, a => a.Expression));
    }
}

public sealed record SelectItem
{
    public required string Field { get; init; }
    public required string Expression { get; init; }
}