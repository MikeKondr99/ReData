namespace ReData.Domain.Query.Lang.Expressions;

public sealed record BooleanLiteral : IExpr
{
    public required bool Value { get; init; }
}