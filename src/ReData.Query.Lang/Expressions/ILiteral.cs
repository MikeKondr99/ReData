namespace ReData.Query.Lang.Expressions;

public interface ILiteral : IExpr
{
    
}

public interface ILiteral<out T> : ILiteral
{
    public T Value { get; }
}