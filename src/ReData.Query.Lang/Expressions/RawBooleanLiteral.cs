namespace ReData.Query.Lang.Expressions;

public record struct RawBooleanLiteral(bool Value) : ILiteral<bool>
{
    public override string ToString()
    {
        return Value ? "true" : "false";
    }
}