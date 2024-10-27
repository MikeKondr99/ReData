using ReData.Query.Visitors;

namespace ReData.Query.Functions;

public record FunctionReturnType
{
    public required DataType DataType { get; init; }

    public required bool CanBeNull { get; init; }
    
    public bool Aggregated { get; init; }
}

public record FunctionDefinition
{
    public required string Name { get; init; }

    public required string? Doc { get; init; }

    public required IReadOnlyList<FunctionArgument> Arguments { get; init; }
    
    public required FunctionReturnType ReturnType { get; init; }
    
    public required FunctionKind Kind { get; init; }

    public required ITemplate Template { get; init; }

    public override string ToString()
    {
        return $"{Name}({
            String.Join(", ", Arguments.Select(a => $"{a.Name}:{a.Type.DataType}{(a.Type.CanBeNull ? "?" : "")}"))
        }) -> {ReturnType.DataType}{(ReturnType.CanBeNull ? "?" : "")}";

    }
}

public enum FunctionKind
{
    Default = 0,
    Method = 1,
    Binary = 2,
    Unary = 3,
}
