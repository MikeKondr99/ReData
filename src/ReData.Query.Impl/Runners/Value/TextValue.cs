namespace ReData.Query.Impl.Runners;

public readonly record struct TextValue(string Value) : IValue
{
    public override string ToString() => $"'{Value}'";
}