using ReData.Core;
using ReData.Query.Functions;

namespace ReData.Query;

public record FunctionSignature
{
    public required string Name { get; init; }
    
    public required FunctionKind Kind { get; init; }

    public required IReadOnlyList<FunctionArgumentType> ArgumentTypes { get; init; }
     
    public override string ToString()
    {
        if (Kind is FunctionKind.Binary)
        {
            return $"({ArgumentTypes[0]} {Name} {ArgumentTypes[1]})";
        }
        return $"{Name}({
            String.Join(", ", ArgumentTypes.Select(a => $"{a}"))
        })";
    }
}