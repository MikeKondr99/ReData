using Pattern.Unions;
using ReData.Query.Visitors;

namespace ReData.DemoApplication;

public class WhereTransformation : ITransformation
{
    public required string Condition { get; set; }

    public Result<QueryBuilder, QueryBuilderError> Apply(QueryBuilder builder)
    {
        return builder.Where(Condition);
    }
}