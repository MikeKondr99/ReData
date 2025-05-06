using System.Globalization;

namespace ReData.Query.Runners.Value;

public readonly record struct DateTimeValue(DateTime Value) : IValue
{
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}