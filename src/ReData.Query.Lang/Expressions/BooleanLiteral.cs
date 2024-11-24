namespace ReData.Query.Lang.Expressions;

public record struct BooleanLiteral(bool Value) : ILiteral<bool>
{
    public override string ToString()
    {
        return Value ? "true" : "false";
    }
}