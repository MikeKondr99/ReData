using ReData.Common;

namespace ReData.Query.Core.Types;

public record FunctionSignature
{
    public required string Name { get; init; }
    
    public required FunctionKind Kind { get; init; }

    public required IReadOnlyList<ExprType> ArgumentTypes { get; init; }
    
     
    public override string ToString()
    {
        if (Kind is FunctionKind.Binary)
        {
            return $"({ArgumentTypes[0]} {Name} {ArgumentTypes[1]})";
        }
        if (Kind is FunctionKind.Unary)
        {
            return $"({Name} {ArgumentTypes[0]})";
        }
        if (Kind is FunctionKind.Method)
        {
            return $"{ArgumentTypes[0]}.{Name}({
                ArgumentTypes.Skip(1).JoinBy(", ")
            })";
        }

        return $"{Name}({
            ArgumentTypes.JoinBy(", ")
        })";
    }
}
