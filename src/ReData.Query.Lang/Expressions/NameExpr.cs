
namespace ReData.Query.Lang.Expressions;

public sealed record NameExpr(string Value) : Expr
{
    public override string ToString()
    {
        return $"[{Value}]";
    }

    public override int GetHashCode()
    {
        return HashCode.Combine("NameExpr", Value);
    }
}