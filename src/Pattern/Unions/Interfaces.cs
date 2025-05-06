namespace Pattern.Unions;

public interface IOk<T>
{
    T Value { get; }

    public void Deconstruct(out T value)
    {
        value = Value;
    }
}

public interface IError<E>
{
    E Value { get; }
    
    public void Deconstruct(out E value)
    {
        value = Value;
    }
}

public interface ISome<T>
{
    T Value { get; }

    public void Deconstruct(out T value)
    {
        value = Value;
    }
}

public interface INone;
