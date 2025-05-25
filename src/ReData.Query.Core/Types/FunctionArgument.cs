using System.Runtime.InteropServices;

namespace ReData.Query.Core.Types;

public record FunctionArgument
{
    public required string Name { get; init; }

    public required FunctionArgumentType Type { get; init; }
    
    public required FunctionArgumentOptions Options { get; init; }
    
    public override string ToString()
    {
        return $"{Name}:{(Options.AllowsOnlyConst() ? " const" : "")} {Type}";
    }
}

public record FunctionArgumentType
{
    public required DataType DataType { get; init; }

    public required bool CanBeNull { get; init; }

    public override string ToString()
    {
        return $"{DataType.Display()}{(CanBeNull ? "" : "!")}";
    }
}

[Flags]
public enum FunctionArgumentOptions
{
    None = 0,
    NotPropagateNull = 1,
    ConstOnly = 2
}

public static class FunctionArgumentOptionsExtensions
{
    public static bool PropagatesNull(this FunctionArgumentOptions options) =>
        !options.HasFlag(FunctionArgumentOptions.NotPropagateNull);
    
    public static bool AllowsOnlyConst(this FunctionArgumentOptions options) =>
        options.HasFlag(FunctionArgumentOptions.ConstOnly);

}
