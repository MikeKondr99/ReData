using System.Globalization;

namespace ReData.DataIO.ValueFormats;

public sealed class IsoDateFormat : IValueFormat
{
    public Type GetValueType() => typeof(DateTime);
    
    public bool TryConvert(string input, out object? value)
    {
        var result = DateTime.TryParseExact(input, "yyyy-MM-dd", 
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var dateValue);
        value = dateValue;
        return result;
    }
}

// Американский формат (MM/dd/yyyy)
public sealed class UsDateFormat : IValueFormat
{
    public Type GetValueType() => typeof(DateTime);
    
    public bool TryConvert(string input, out object? value)
    {
        var result = DateTime.TryParseExact(input, "MM/dd/yyyy", 
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var dateValue);
        value = dateValue;
        return result;
    }
}

// Европейский формат (dd.MM.yyyy)
public sealed class EuroDateFormat : IValueFormat
{
    public Type GetValueType() => typeof(DateTime);
    
    public bool TryConvert(string input, out object? value)
    {
        var result = DateTime.TryParseExact(input, "dd.MM.yyyy", 
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var dateValue);
        value = dateValue;
        return result;
    }
}

// Короткий год (dd/MM/yy)
public sealed class ShortYearDateFormat : IValueFormat
{
    public Type GetValueType() => typeof(DateTime);
    
    public bool TryConvert(string input, out object? value)
    {
        var result = DateTime.TryParseExact(input, "dd.MM.yy", 
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var dateValue);
        value = dateValue;
        return result;
    }
}

// Дата и время без часового пояса (yyyy-MM-dd HH:mm:ss)
public sealed class DateTimeNoZoneFormat : IValueFormat
{
    public Type GetValueType() => typeof(DateTime);
    
    public bool TryConvert(string input, out object? value)
    {
        var result = DateTime.TryParseExact(input, "yyyy-MM-dd HH:mm:ss", 
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var dateValue);
        value = dateValue;
        return result;
    }
}