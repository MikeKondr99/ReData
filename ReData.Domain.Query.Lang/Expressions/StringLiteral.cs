namespace ReData.Domain.Query.Lang.Expressions;

public record StringLiteral : IExpr
{
    public StringLiteral() {}

    public StringLiteral(string value)
    {
        Value = value;
    }
    public required string Value { get; init; }

}