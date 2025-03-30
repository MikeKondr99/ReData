using System.Globalization;

namespace ReData.Query.Impl.Runners.Value;

public readonly record struct NumberValue(double Value) : IValue
{
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}


public readonly record struct DateTimeValue(DateTime Value) : IValue
{
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}