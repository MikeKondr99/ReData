using ReData.Query.Lang.Expressions;

namespace ReData.Query.Visitors;

public interface IExprVisitor<T>
{
    public T Visit(IExpr expr);
}