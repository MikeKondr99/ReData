using System.Runtime.CompilerServices;
using ReData.Core;
using ReData.Query.Functions;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Visitors;

public class TypeVisitor : ExprVisitor<ExprType>, ITypeVisitor
{
    public required IFieldStorage FieldTypes { get; init; }
    
    public required IFunctionStorage FunctionTypes { get; init; }

    public override ExprType Visit(IRawExpr rawExpr)
    {
        var result = rawExpr switch
        {
            StringLiteral s => Visit(s),
            RawNumberLiteral n => Visit(n),
            RawIntegerLiteral i => Visit(i),
            RawBooleanLiteral b => Visit(b),
            RawNullRawLiteral nl => Visit(nl),
            NameRawExpr n => FieldTypes.GetType(n.Value),
            FuncRawExpr f => Visit(f),
            var unmatched => throw new UnmatchedException<IRawExpr>(unmatched)
        };
        return result;
    }

    public override ExprType Visit(StringLiteral expr) => ExprType.Text().Const();
    public override ExprType Visit(RawNumberLiteral expr) => ExprType.Number().Const();
    public override ExprType Visit(RawIntegerLiteral expr) => ExprType.Integer().Const();
    public override ExprType Visit(RawBooleanLiteral expr) => ExprType.Boolean().Const();
    
    public override ExprType Visit(RawNullRawLiteral expr) => ExprType.Null();
    
    public override ExprType Visit(FuncRawExpr rawExpr)
    {
        ExprType[] types = new ExprType[rawExpr.Arguments.Count];
        
        for (int i = 0; i < rawExpr.Arguments.Count(); i++)
        {
            types[i] = Visit(rawExpr.Arguments[i]);
        }
        
        var sign = new FunctionSignature
        {
            Name = rawExpr.Name,
            Kind = rawExpr.Kind switch
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
    

    public override ExprType Visit(NameRawExpr nameRaw)
    {
        return FieldTypes.GetType(nameRaw.Value);
    }
    
    
}

public interface ITypeVisitor : IExprVisitor<ExprType>
{
    
}

public static class TypeResolver
{
    
}