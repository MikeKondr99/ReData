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
         return $"{Name}({String.Join(", ", ArgumentTypes.JoinBy(", "))})";
     }
}