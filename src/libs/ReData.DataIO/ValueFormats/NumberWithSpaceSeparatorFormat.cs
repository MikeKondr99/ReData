using System.Globalization;

namespace ReData.DataIO.ValueFormats;

public sealed class NumberWithSpaceSeparatorFormat : IValueFormat
{
    public Type GetValueType() => typeof(decimal);
    
    public bool TryConvert(string input, out object? value)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            value = null;
            return true;
        }
        
        // Убираем пробелы и пробуем распарсить
        var cleaned = input.Replace(" ", "");
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