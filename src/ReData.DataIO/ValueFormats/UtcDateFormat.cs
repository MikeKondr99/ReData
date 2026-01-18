namespace ReData.DataIO.ValueFormats;

public sealed class UtcDateFormat : IValueFormat
{
    public Type GetValueType() => typeof(DateTime);
    
    public bool TryConvert(string input, out object? value)
    {
            
        var result = DateTime.TryParseExact(input, "yyyy-MM-ddTHH:mm:ssZ", 
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AssumeUniversal,
            out var dateValue);
        value = dateValue;
        return result;
    }
}