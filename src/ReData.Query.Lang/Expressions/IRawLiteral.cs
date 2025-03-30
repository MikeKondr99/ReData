namespace ReData.Query.Lang.Expressions;

public interface IRawLiteral : IRawExpr
{
    
}

public interface ILiteral<out T> : IRawExpr
{
    public T Value { get; }
}