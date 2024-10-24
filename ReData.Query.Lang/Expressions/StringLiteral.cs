namespace ReData.Query.Lang.Expressions;

public record struct StringLiteral(string Value) : ILiteral<string>
{
    public override string ToString()
    {
        return $"'{Value}'";
    }
}