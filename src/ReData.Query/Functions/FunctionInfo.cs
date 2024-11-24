using System.Security.Cryptography.X509Certificates;
using ReData.Query.Visitors;

namespace ReData.Query.Functions;

public sealed record FunctionReturnType
{
    public required DataType DataType { get; init; }

    public required bool CanBeNull { get; init; }
    
    public bool Aggregated { get; init; }

    public override string ToString()
    {
        return $"{DataType}{(CanBeNull ? "?" : "")}";
    }
}



public sealed record FunctionDefinition
{
    public required string Name { get; init; }

    public required string? Doc { get; init; }

    public required IReadOnlyList<FunctionArgument> Arguments { get; init; }
    
    public required FunctionReturnType ReturnType { get; init; }
    
    public required FunctionKind Kind { get; init; }

    public required ITemplate Template { get; init; }
    
    public required ImplicitCastMetadata? ImplicitCast { get; init; }
    
    public required Func<bool[], bool>? NullIf { get; init; }

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

public sealed class ImplicitCastMetadata
{
    public uint Cost { get; init; }
    
}
public enum FunctionKind
{
    Default = 0,
    Method = 1,
    Binary = 2,
    Unary = 3,
}
