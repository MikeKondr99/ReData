namespace ReData.Domain.Query.Lang.Expressions;

public sealed record IntegerLiteral : IExpr
{
    public required long Value { get; init; }
    
}