namespace ReData.Query.Lang.Expressions;

public class FuncExpr : IExpr
{
    public required string Name { get; init; }
    
    public required IExpr[] Arguments { get; init; }

}