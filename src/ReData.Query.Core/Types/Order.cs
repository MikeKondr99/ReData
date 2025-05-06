using ReData.Query.Core.Template;

namespace ReData.Query.Core.Types;

public record struct Order(ResolvedExpr ResolvedExpr, Order.Type Direction)
{
    public enum Type
    {
        Asc,
        Desc
    }
}