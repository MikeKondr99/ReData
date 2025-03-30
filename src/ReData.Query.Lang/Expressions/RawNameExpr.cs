namespace ReData.Query.Lang.Expressions;

public record struct NameRawExpr(string Value) : IRawExpr
{
    public override string ToString()
    {
        return $"[{Value}]";
    }
}