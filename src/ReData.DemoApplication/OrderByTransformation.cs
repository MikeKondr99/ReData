
using ReData.Query.Visitors;
using ReData.Query;

namespace ReData.DemoApplication;

public class OrderByTransformation : ITransformation
{
    public required string Expression { get; set; }
    public required bool Descending { get; set; }

    public QueryBuilder Apply(QueryBuilder builder)
    {
        return builder.OrderBy([(Expression, Descending ? Query.Query.Order.Type.Desc : Query.Query.Order.Type.Asc)]);
    }
}