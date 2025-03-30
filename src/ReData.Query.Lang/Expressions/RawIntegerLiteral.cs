namespace ReData.Query.Lang.Expressions;

public record struct RawIntegerLiteral(long Value) : ILiteral<long>
{
    public override string ToString()
    {
        return Value.ToString();
    }
}