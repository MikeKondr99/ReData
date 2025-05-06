using System.Globalization;

namespace ReData.Query.Runners.Value;

public readonly record struct NumberValue(double Value) : IValue
{
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}