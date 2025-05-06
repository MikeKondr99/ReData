using Dunet;

namespace Pattern.Unions;

public readonly record struct None;

public static class Option
{
    public static Option<T>.Some Some<T>(T value) => new(value);

    public static Option<T>.None None<T>() => new();
    
    public static None None() => new();
}

[Union]
public abstract partial record Option<T>
{
    partial record Some(T Value) : ISome<T>
    {
        public static implicit operator T(Some self) => self.Value;
    }

    partial record None() : INone
    {
        public static implicit operator T?(None self) => default;
    }
    
    public static implicit operator Option<T>(T value)
    {
        return new Some(value);
    }
    
    public static implicit operator Option<T>(Unions.None value)
    {
        return new None();
    }
}