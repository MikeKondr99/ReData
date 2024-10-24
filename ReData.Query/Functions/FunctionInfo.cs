using ReData.Query.Visitors;

namespace ReData.Query.Functions;

public struct FunctionInfo
{
    public required string Name;
    public required IReadOnlyList<FunctionArgument> Arguments;
    public required ExprType ReturnType;
    public required ITemplate Template;
}

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
    

    public required ITemplate Template { get; init; }

    public override string ToString()
    {
        return $"{Name}({
            String.Join(", ", Arguments.Select(a => $"{a.Name}:{a.Type.DataType}{(a.Type.CanBeNull ? "?" : "")}"))
        }) -> {ReturnType.DataType}{(ReturnType.CanBeNull ? "?" : "")}";

    }
}
