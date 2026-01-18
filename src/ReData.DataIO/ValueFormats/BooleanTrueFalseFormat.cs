namespace ReData.DataIO.ValueFormats;

public sealed class BooleanTrueFalseFormat : IValueFormat
{
    public Type GetValueType() => typeof(bool);
    
    public bool TryConvert(string input, out object? value)
    {
        var lower = input.ToLowerInvariant();
        
        if (lower == "true")
        {
            value = true;
            return true;
        }
        
        if (lower == "false")
        {
            value = false;
            return true;
        }
        
        value = null;
        return false;
    }
}