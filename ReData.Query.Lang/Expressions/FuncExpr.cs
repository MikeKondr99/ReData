namespace ReData.Query.Lang.Expressions;

public sealed record FuncExpr : IExpr
{
    public required string Name { get; init; }
    
    public required IExpr[] Arguments { get; init; }

}