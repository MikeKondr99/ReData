using ReData.Query.Visitors;

namespace ReData.DemoApplication;

public class SelectTransformation : ITransformation
{
    public required Dictionary<string,string> Mapping { get; init; }

    public QueryBuilder Apply(QueryBuilder builder)
    {
        return builder.Select(Mapping);
    }
}