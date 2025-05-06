namespace Pattern.Unions;

public readonly record struct ResultError<E>(E Value)
{
    public Result<T, E> WithOk<T>() => this;
}