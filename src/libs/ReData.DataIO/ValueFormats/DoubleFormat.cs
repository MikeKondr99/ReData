namespace ReData.DataIO.ValueFormats;

public sealed class DoubleFormat : IValueFormat
{
    public Type GetValueType() => typeof(double);
    
    public bool TryConvert(string input, out object? value)
    {
        var result = double.TryParse(input, 
            System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.InvariantCulture,
            out var decimalValue);
        value = decimalValue;
        return result;
    }
}