using ReData.Query.Core.Template;

namespace ReData.Query.Core.Types;

public sealed record FunctionDefinition
{
    public required string Name { get; init; }

    public required string? Doc { get; init; }

    public required IReadOnlyList<FunctionArgument> Arguments { get; init; }
    
    public required FunctionReturnType ReturnType { get; init; }
    
    public required FunctionKind Kind { get; init; }

    public required ITemplate Template { get; init; }
    
    public required ImplicitCastMetadata? ImplicitCast { get; init; }

    public required Func<IEnumerable<bool>, bool>? CustomNullPropagation;
    
    public override string ToString()
    {
        if (Kind is FunctionKind.Binary)
        {
            return $"({Arguments[0].Type} {Name} {Arguments[1].Type}) -> {ReturnType}";
        }
        return $"{Name}({
            String.Join(", ", Arguments.Select(a => $"{a}"))
        }) -> {ReturnType}";
    }
}