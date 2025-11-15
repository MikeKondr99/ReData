using Dunet;
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

    public required Func<IEnumerable<bool>, bool>? CustomNullPropagation { get; init; }

    public required ConstPropagation ConstPropagation { get; init; }


    private string? cacheToString;

    public override string ToString()
    {
        if (cacheToString is null)
        {
            if (Kind is FunctionKind.Binary)
            {
                cacheToString =  $"({Arguments[0].Type} {Name} {Arguments[1].Type}) -> {ReturnType}";
            }
            else
            {
                cacheToString = $"{Name}({string.Join(", ", Arguments.Select(a => $"{a}"))}) -> {ReturnType}";
            }
        }
        return cacheToString;
    }
}

public enum ConstPropagation
{
    Default = 1,
    AlwaysTrue = 2,
    AlwaysFalse = 3,
}