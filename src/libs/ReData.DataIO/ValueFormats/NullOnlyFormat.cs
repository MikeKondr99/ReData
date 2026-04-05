namespace ReData.DataIO.ValueFormats;

public sealed class NullOnlyFormat : IValueFormat
{
    public Type GetValueType() => typeof(string);
    
    public bool TryConvert(string input, out object? value)
    {
        value = null;
        return false;
    }
}