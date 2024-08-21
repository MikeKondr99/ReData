namespace ReData.Domain.Query.Lang.Expressions;

public record NumberLiteral : IExpr
{
    public required double Value { get; init; }
}