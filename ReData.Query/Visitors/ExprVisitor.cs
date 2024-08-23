using ReData.Query.Lang.Expressions;

namespace ReData.Query.Visitors;

public abstract class ExprVisitor<T> : IExprVisitor<T>
{
    public virtual T Visit(IExpr expr)
    {
        return expr switch
        {
            StringLiteral s => Visit(s),
            NumberLiteral n => Visit(n),
            IntegerLiteral i => Visit(i),
            BooleanLiteral b => Visit(b),
            NullLiteral nl => Visit(nl),
            NameExpr n => Visit(n),
            FuncExpr f => Visit(f),
        };
    }

    public abstract T Visit(StringLiteral expr);

    public abstract T Visit(NumberLiteral expr);

    public abstract T Visit(IntegerLiteral expr);

    public abstract T Visit(BooleanLiteral expr);
    
    public abstract T Visit(NameExpr expr);

    public abstract T Visit(NullLiteral expr);

    public abstract T Visit(FuncExpr expr);

}