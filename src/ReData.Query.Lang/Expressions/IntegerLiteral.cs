namespace ReData.Query.Lang.Expressions;

public record struct IntegerLiteral(long Value) : ILiteral<long>
{
    public override string ToString()
    {
        return Value.ToString();
    }
}