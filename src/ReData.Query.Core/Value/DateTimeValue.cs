using System.Globalization;

namespace ReData.Query.Core.Value;

public readonly record struct DateTimeValue(DateTime Value) : IValue
{
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}