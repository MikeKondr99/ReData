namespace ReData.DataIO.ValueFormats;

public interface IValueFormat
{
    public Type GetValueType();
    
    public bool TryConvert(string input, out object? value);
}
