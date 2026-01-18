using System.Globalization;

namespace ReData.DataIO.ValueFormats;

public sealed class NumberWithCommaDecimalFormat : IValueFormat
{
    public Type GetValueType() => typeof(decimal);
    
    public bool TryConvert(string input, out object? value)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            value = null;
            return true;
        }
        
        // Заменяем запятую на точку для парсинга
        var normalized = input.Replace(",", ".");
        // Убираем пробелы как разделители тысяч
        normalized = normalized.Replace(" ", "");
        
        if (decimal.TryParse(normalized, NumberStyles.Number, 
                CultureInfo.InvariantCulture, out var decimalValue))
        {
            value = decimalValue;
            return true;
        }
        
        value = null;
        return false;
    }
}