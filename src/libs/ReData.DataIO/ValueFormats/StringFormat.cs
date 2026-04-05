namespace ReData.DataIO.ValueFormats;

public sealed class StringFormat : IValueFormat
{
    public Type GetValueType() => typeof(string);
    
    public bool TryConvert(string input, out object? value)
    {
        value = input;
        return true;
    }
}