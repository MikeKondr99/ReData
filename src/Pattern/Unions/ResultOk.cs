namespace Pattern.Unions;

public readonly record struct ResultOk<T>(T Value)
{
    public Result<T, E> WithError<E>() => this;

}