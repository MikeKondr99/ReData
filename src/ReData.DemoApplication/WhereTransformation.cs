using ReData.Query.Visitors;

namespace ReData.DemoApplication;

public class WhereTransformation : ITransformation
{
    public required string Condition { get; set; }

    public QueryBuilder Apply(QueryBuilder builder)
    {
        return builder.Where(Condition);
    }
}