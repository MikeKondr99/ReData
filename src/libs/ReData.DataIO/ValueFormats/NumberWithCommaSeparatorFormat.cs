using System.Globalization;

namespace ReData.DataIO.ValueFormats;

public sealed class NumberWithCommaSeparatorFormat : IValueFormat
{
    public Type GetValueType() => typeof(decimal);
    
    public bool TryConvert(string input, out object? value)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            value = null;
            return true;
        }
        
        // Убираем запятые как разделители тысяч
        var cleaned = input.Replace(",", "");
        if (decimal.TryParse(cleaned, NumberStyles.Number, 
                CultureInfo.InvariantCulture, out var decimalValue))
        {
            value = decimalValue;
            return true;
        }
        
        value = null;
        return false;
    }
}
