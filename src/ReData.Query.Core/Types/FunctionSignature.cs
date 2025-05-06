namespace ReData.Query.Core.Types;

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
        } else if (Kind is FunctionKind.Unary)
        {
            return $"({Name} {ArgumentTypes[1]})";
        } else if (Kind is FunctionKind.Method)
        {
            return $"{ArgumentTypes[0]}.{Name}({
                String.Join(", ", ArgumentTypes.Skip(1).Select(a => $"{a}"))
            })";
        }
        return $"{Name}({
            String.Join(", ", ArgumentTypes.Select(a => $"{a}"))
        })";
    }
}