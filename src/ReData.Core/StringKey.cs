namespace ReData.Core;

/// <summary>
/// Строковый ключ для Dictionary который не учитывает регистр строки, но при этом сохраняет его
/// </summary>
sealed public class StringKey
{
    public StringKey(string value)
    {
        Value = value;
    }
    
    public string Value { get; private init; }
    

    public override string ToString()
    {
        return Value;
    }

    public override bool Equals(object? obj)
    {
        if (obj is StringKey key)
        {
            return this.Value.ToLower() == key.Value.ToLower();
        }

        if (obj is string str)
        {
            return this.Value.ToLower() == str.ToLower();
        }

        return false;
    }

    public override int GetHashCode()
    {
        return Value.ToLower().GetHashCode();
    }

    public static implicit operator StringKey(string value)
    {
        return new StringKey(value);
    }

    public static implicit operator string(StringKey value)
    {
        return value.Value;
    }
}

public sealed class UnmatchedException<T>(T unknown) : Exception
{
    public override string Message => unknown is null 
        ? $"While mathching {typeof(T)} null occured"
        : $"While mathching {typeof(T)} unknown variant {unknown.GetType().Name} occured";
}

public sealed class UnknownEnumValueException<T>(T unknown) : Exception
where T : Enum
{
    public override string Message => $"While mathching enum {typeof(T).Name} unknown variant {unknown} occured";
}
