using Pattern;
using Pattern.Unions;
using ReData.Query.Core.Components;
using ReData.Query.Core.Components.Implementation;
using ReData.Query.Core.Template;
using ReData.Query.Core.Types;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Core;

public sealed class ExpressionResolver : INameResolver
{
    public required ILiteralResolver LiteralResolver { private get; init; }
    public required INameResolver NameResolver { private get; init; }
    public required IFunctionStorage Functions { private get; init; }

    public Result<ResolvedExpr, ExprError> ResolveExpr(Expr expr, IFieldStorage fields)
    {
        var result = expr switch
        {
            Literal literal => ResolveLiteral(literal),
            NameExpr name => ResolveName(name, fields),
            FuncExpr func => ResolveFunction(func, fields),
        };
        return result;
    }


    public ResolvedExpr ResolveLiteral(Literal literal)
    {
        return LiteralResolver.Resolve(literal);
    }

    public Result<ResolvedExpr, ExprError> ResolveName(NameExpr name, IFieldStorage fields)
    {
        var fieldOption = fields[name.Value];
        if (fieldOption is ISome<Field>(var field))
        {
            return new ResolvedExpr
            {
                Expression = name,
                Template = field.Template,
                Type = new ExprType()
                {
                    DataType = field.Type.Type,
                    CanBeNull = field.Type.CanBeNull,
                    IsConstant = false,
                    Aggregated = false,
                },
            };
        }

        return new ExprError()
        {
            Span = name.Span,
            Message = $"Field '{name.Value} was not found"
        };
    }

    
    public Result<ResolvedExpr, ExprError> ResolveFunction(FuncExpr funcExpr, IFieldStorage fields)
    {
        var arguments = funcExpr.Arguments.Select(a => ResolveExpr(a, fields)).ToResult();

        if (!arguments.Unwrap(out var args, out var error))
        {
            return error;
        }
        
        var sign = new FunctionSignature
        {
            Name = funcExpr.Name,
            Kind = MapFunctionKind(funcExpr.Kind),
            ArgumentTypes = args.Select(arg => arg.Type).ToArray()
        };
        var def = Functions.ResolveFunction(sign);
        if (def.UnwrapErr(out var err, out var definition))
        {
            return new ExprError()
            {
                Span = funcExpr.Span,
                Message = err.Message,
            };
        }
        
        var function = definition.Function;

        return new ResolvedExpr()
        {
            Expression = funcExpr,
            Template = function.Template,
            Type = new ExprType()
            {
                DataType = function.ReturnType.DataType,
                CanBeNull = definition.PropagatesNull,
                IsConstant = definition.ReturnsConst,
                Aggregated = definition.ReturnsAggregated,
            },
            Arguments = args.ToArray()
        };
    }

    private FunctionKind MapFunctionKind(FuncExprKind kind) => kind switch
    {
        FuncExprKind.Binary => FunctionKind.Binary,
        FuncExprKind.Method => FunctionKind.Method,
        FuncExprKind.Unary => FunctionKind.Unary,
        FuncExprKind.Default => FunctionKind.Default,
        var a => (FunctionKind)a,
    };
    
    public TableTemplate ResolveName(ReadOnlySpan<string> path)
    {
        return NameResolver.ResolveName(path);
    }

}
