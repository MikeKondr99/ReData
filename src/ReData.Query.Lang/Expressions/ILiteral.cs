namespace ReData.Query.Lang.Expressions;

public interface ILiteral : IExpr
{
    
}

public interface ILiteral<out T> : IExpr
{
    public T Value { get; }
}