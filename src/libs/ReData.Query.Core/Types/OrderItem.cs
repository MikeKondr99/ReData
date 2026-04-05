using ReData.Query.Core.Template;

namespace ReData.Query.Core.Types;

public record struct OrderItem(ResolvedExpr ResolvedExpr, OrderItem.Type Direction)
{
    public enum Type
    {
        Asc,
        Desc
    }
}