namespace ReData.Query.Lang.Expressions;

public record struct NullLiteral : ILiteral
{
    public override string ToString()
    {
        return "null";
    }
}