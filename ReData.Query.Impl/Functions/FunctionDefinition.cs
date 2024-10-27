using ReData.Query.Functions;
using ReData.Query.Visitors;

namespace ReData.Query.Impl.Functions;

public record FunctionDefinition
{
    public required string Name { get; init; }

    public required string? Doc { get; init; }

    public required IReadOnlyList<FunctionArgument> Arguments { get; init; }

    public required FunctionReturnType ReturnType { get; init; }
    
    public required FunctionKind Kind { get; init; }
    
    public required IReadOnlyDictionary<DatabaseTypeFlags, ITemplate> Templates { get; init; }
    

    public override string ToString()
    {
        return $"{Name}({
            String.Join(", ", Arguments.Select(a => $"{a.Name}:{a.Type.DataType}{(a.Type.CanBeNull ? "?" : "")}"))
        }) -> {ReturnType.DataType}{(ReturnType.CanBeNull ? "?" : "")}";
    }
}