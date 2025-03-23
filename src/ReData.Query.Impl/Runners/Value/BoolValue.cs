using System.Globalization;

namespace ReData.Query.Impl.Runners;

public readonly record struct BoolValue(bool Value) : IValue
{
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}