namespace ReData.Query.Runners.Value;

public readonly record struct UnknownValue(string Type) : IValue
{
    public override string ToString() => $"UNKNOWN({Type})";
}