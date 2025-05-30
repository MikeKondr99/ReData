using ReData.Query.Core.Template;
using ReData.Query.Core.Types;

namespace ReData.Query.Impl.Functions;

public record FunctionDefinition
{
    public required string Name { get; init; }

    public required string? Doc { get; init; }

    public required IReadOnlyList<FunctionArgument> Arguments { get; init; }

    public required FunctionReturnType ReturnType { get; init; }
    
    public required FunctionKind Kind { get; init; }
    
    public required IReadOnlyDictionary<DatabaseTypeFlags, ITemplate> Templates { get; init; }
    
    public ImplicitCastMetadata? ImplicitCast { get; init; }
    
    public required Func<IEnumerable<bool>, bool>? CustomNullPropagation { get; init; }
    
    public required ConstPropagation ConstPropagation { get; init; }

    public override string ToString()
    {
        if (Kind is FunctionKind.Binary)
        {
            return $"({Arguments[0].Type} {Name} {Arguments[1].Type}) -> {ReturnType}";
        }
        return $"{Name}({ String.Join(", ", Arguments.Select(a => $"{a}")) }) -> {ReturnType}";
    }
}
