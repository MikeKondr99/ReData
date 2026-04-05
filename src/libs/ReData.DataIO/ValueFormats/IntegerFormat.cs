namespace ReData.DataIO.ValueFormats;

public sealed class IntegerFormat : IValueFormat
{
    public Type GetValueType() => typeof(long);
    
    public bool TryConvert(string input, out object? value)
    {
        var result = long.TryParse(input, out var intValue);
        value = intValue;
        return result;
    }
}