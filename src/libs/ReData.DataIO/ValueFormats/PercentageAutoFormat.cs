using System.Globalization;

namespace ReData.DataIO.ValueFormats;

public sealed class PercentageAutoFormat : IValueFormat
{
    public Type GetValueType() => typeof(decimal);
    
    public bool TryConvert(string input, out object? value)
    {
        if (!input.EndsWith('%'))
        {
            value = null;
            return false;
        }
        
        var cleaned = input.TrimEnd('%');
        if (decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out var decimalValue))
        {
            // Проверяем что число между 0 и 100 для валидных процентов
            if (decimalValue >= 0 && decimalValue <= 100)
            {
                value = decimalValue / 100m;
                return true;
            }
        }
        
        value = null;
        return false;
    }
}