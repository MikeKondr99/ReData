
namespace ReData.Query.Lang.Expressions;

public sealed record NameExprNode(string Value) : ExprNode
{
    public override string ToString()
    {
        return $"[{Value}]";
    }

    public override int GetHashCode()
    {
        return HashCode.Combine("NameExprNode", Value);
    }
}