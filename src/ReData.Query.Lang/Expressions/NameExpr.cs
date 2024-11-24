namespace ReData.Query.Lang.Expressions;

public record struct NameExpr(string Value) : IExpr
{
    public override string ToString()
    {
        return $"[{Value}]";
    }
}