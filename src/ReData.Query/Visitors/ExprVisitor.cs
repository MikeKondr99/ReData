using ReData.Core;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Visitors;

public abstract class ExprVisitor<T> : IExprVisitor<T>
{
    public virtual T Visit(IRawExpr rawExpr)
    {
        return rawExpr switch
        {
            StringLiteral s => Visit(s),
            RawNumberLiteral n => Visit(n),
            RawIntegerLiteral i => Visit(i),
            RawBooleanLiteral b => Visit(b),
            RawNullRawLiteral nl => Visit(nl),
            NameRawExpr n => Visit(n),
            FuncRawExpr f => Visit(f),
            var unmatched => throw new UnmatchedException<IRawExpr>(unmatched)
        };
    }

    public abstract T Visit(StringLiteral expr);

    public abstract T Visit(RawNumberLiteral expr);

    public abstract T Visit(RawIntegerLiteral expr);

    public abstract T Visit(RawBooleanLiteral expr);
    
    public abstract T Visit(NameRawExpr rawExpr);

    public abstract T Visit(RawNullRawLiteral expr);

    public abstract T Visit(FuncRawExpr rawExpr);

}