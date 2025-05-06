using ReData.Common;

namespace ReData.Query.Lang.Expressions;

public record FuncExpr : Expr
{
    public required string Name { get; init; }
    
    public required IReadOnlyList<Expr> Arguments { get; init; }
    
    public FuncExprKind Kind { get; init; }
    
    public override string ToString()
    {
        return Kind switch
        {
            FuncExprKind.Binary => $"({Arguments[0]} {Name} {Arguments[1]})",
            FuncExprKind.Unary => $"({Name} {Arguments[1]})",
            FuncExprKind.Method => $"{Arguments[0]}.{Name}({Arguments.Skip(1).JoinBy(", ")})",
            FuncExprKind.Default => $"{Name}({Arguments.JoinBy(", ")})",
            _ => $"{Name}({Arguments.JoinBy(", ")})",
        };
    }
}

public enum FuncExprKind
{
    Default = 0,
    Binary = 1,
    Unary = 2,
    Method = 3,
}