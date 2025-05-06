using ReData.Query.Runners.Value;

namespace ReData.Query.Runners.Value;

public readonly record struct NullValue : IValue
{
    public override string ToString() => "NULL";
}