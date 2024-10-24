using ReData.Core;

namespace ReData.Query.Lang.Expressions;

public record struct FuncExpr : IExpr
{
    public required string Name { get; init; }
    
    public required IExpr[] Arguments { get; init; }

    public override string ToString()
    {
        return $"{Name}({Arguments.JoinBy(", ")})";
    }
}