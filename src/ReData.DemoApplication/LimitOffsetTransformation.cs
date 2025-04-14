using System.Runtime.CompilerServices;
using ReData.Query.Visitors;

namespace ReData.DemoApplication;

public class LimitOffsetTransformation : ITransformation
{
    public uint? Take { get; set; }
    public uint? Skip { get; set; }

    public QueryBuilder Apply(QueryBuilder builder)
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