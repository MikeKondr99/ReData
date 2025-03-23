using System.Runtime.CompilerServices;
using ReData.Core;
using ReData.Query.Functions;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Visitors;

public class TypeVisitor : ExprVisitor<ExprType>, ITypeVisitor
{
    public required IFieldStorage FieldTypes { get; init; }
    
    public required IFunctionStorage FunctionTypes { get; init; }

    public override ExprType Visit(IExpr expr)
    {
        var result = expr switch
        {
            StringLiteral s => Visit(s),
            NumberLiteral n => Visit(n),
            IntegerLiteral i => Visit(i),
            BooleanLiteral b => Visit(b),
            NullLiteral nl => Visit(nl),
            NameExpr n => FieldTypes.GetType(n.Value),
            FuncExpr f => Visit(f),
            var unmatched => throw new UnmatchedException<IExpr>(unmatched)
        };
        return result;
    }

    public override ExprType Visit(StringLiteral expr) => ExprType.Text().Const();
    public override ExprType Visit(NumberLiteral expr) => ExprType.Number().Const();
    public override ExprType Visit(IntegerLiteral expr) => ExprType.Integer().Const();
    public override ExprType Visit(BooleanLiteral expr) => ExprType.Boolean().Const();
    
    public override ExprType Visit(NullLiteral expr) => ExprType.Null();
    
    public override ExprType Visit(FuncExpr expr)
    {
        ExprType[] types = new ExprType[expr.Arguments.Count];
        
        for (int i = 0; i < expr.Arguments.Count(); i++)
        {
            types[i] = Visit(expr.Arguments[i]);
        }
        
        var sign = new FunctionSignature
        {
            Name = expr.Name,
            Kind = expr.Kind switch
            {
                FuncExprKind.Binary => FunctionKind.Binary,
                FuncExprKind.Method => FunctionKind.Method,
                FuncExprKind.Unary => FunctionKind.Unary,
                FuncExprKind.Default => FunctionKind.Default,
                var a => (FunctionKind)a,
            },
            ArgumentTypes = types.Select(t => new FunctionArgumentType()
            {
                DataType = t.Type,
                CanBeNull = t.CanBeNull,
            }).ToArray()
        };
        var res  = FunctionTypes.ResolveFunction(sign);
        if (res is null)
        {
            throw new Exception($"function `{sign} not found");
        }
        
        return new ExprType()
        {
            Type = res.Function.ReturnType.DataType,
            CanBeNull = PropagatesNull(res.Function, sign),
            IsConstant = types.All(t => t.IsConstant),
        };
    }

    private bool PropagatesNull(FunctionDefinition function, FunctionSignature sign)
    {
        // Если не может быть null значит не может
        if (!function.ReturnType.CanBeNull) return false;

        // Если спец спец правило смотрим по нему
        if (function.CustomNullPropagation is not null)
        {
            return function.CustomNullPropagation(sign.ArgumentTypes.Select(a => a.CanBeNull));
        }

        // Если любой параметре прокидывает null и может быть null.
        for (int i = 0; i < function.Arguments.Count; i++)
        {
            if (function.Arguments[i].PropagateNull && sign.ArgumentTypes[i].CanBeNull)
            {
                return true;
            }
        }
        
        return false;
    }
    

    public override ExprType Visit(NameExpr name)
    {
        return FieldTypes.GetType(name.Value);
    }
    
    
}

public interface ITypeVisitor : IExprVisitor<ExprType>
{
    
}

public static class TypeResolver
{
    
}