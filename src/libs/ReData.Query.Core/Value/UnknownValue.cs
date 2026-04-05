namespace ReData.Query.Core.Value;

public readonly record struct UnknownValue(string Type) : IValue
{
    public override string ToString() => $"UNKNOWN({Type})";
}