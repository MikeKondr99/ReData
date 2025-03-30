namespace ReData.Query.Lang.Expressions;

public record struct RawNullRawLiteral : IRawLiteral
{
    public override string ToString()
    {
        return "null";
    }
}