namespace ReData.DataIO.ValueFormats;

public sealed class BooleanNumericFormat : IValueFormat
{
    public Type GetValueType() => typeof(bool);
    
    public bool TryConvert(string input, out object? value)
    {
        if (input == "1")
        {
            value = true;
            return true;
        }
        
        if (input == "0")
        {
            value = false;
            return true;
        }
        
        value = null;
        return false;
    }
}