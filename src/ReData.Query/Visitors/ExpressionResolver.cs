using System.Globalization;
using ReData.Core;
using ReData.Query.Functions;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Visitors;

public sealed class ExpressionResolver
{
    public required ILiteralResolver LiteralResolver { get; init; }
    public required INameResolver NameResolver { get; init; }
    public required IFunctionStorage Functions { get; init; }

    public Node ResolveExpr(IExpr expr, IFieldStorage fields)
    {
        var result = expr switch
        {
            ILiteral literal => ResolveLiteral(literal),
            NameExpr name => ResolveName(name, fields),
            FuncExpr func => ResolveFunction(func, fields),
            var unmatched => throw new UnmatchedException<IExpr>(unmatched)
        };
        return result;
    }


    public Node ResolveLiteral(ILiteral literal)
    {
        return LiteralResolver.Resolve(literal);
    }

    public Node ResolveName(NameExpr name, IFieldStorage fields)
    {
        var field = fields[name.Value];
        return new Node
        {
            Expression = name,
            Template = field.Template,
            Type = new ExprType()
            {
                Type = field.Type.Type,
                CanBeNull = field.Type.CanBeNull,
                IsConstant = false,
                Aggregated = false,
            },
        };
    }

    
    public Node ResolveFunction(FuncExpr funcExpr, IFieldStorage fields)
    {
        IEnumerable<Node> arguments = funcExpr.Arguments.Select(a => ResolveExpr(a, fields)).ToArray();

        var sign = new FunctionSignature
        {
            Name = funcExpr.Name,
            Kind = MapFunctionKind(funcExpr.Kind),
            ArgumentTypes = arguments.Select(arg => new FunctionArgumentType()
            {
                DataType = arg.Type.Type,
                CanBeNull = arg.Type.CanBeNull,
            }).ToArray()
        };
        var definition = Functions.ResolveFunction(sign) ?? throw new Exception($"function `{sign} not found");
        var function = definition.Function;


        return new Node()
        {
            Expression = funcExpr,
            Template = function.Template,
            Type = new ExprType()
            {
                Type = function.ReturnType.DataType,
                CanBeNull = PropagatesNull(function, sign),
                IsConstant = arguments.All(arg => arg.Type.IsConstant),
            },
            Arguments = arguments.ToArray()
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

    private bool PropagatesNull(FunctionDefinition function, FunctionSignature sign)
    {
        // Если не может быть null значит не может
        if (!function.ReturnType.CanBeNull) return false;

        // Если спец правило смотрим по нему
        if (function.CustomNullPropagation is not null)
        {
            return function.CustomNullPropagation(sign.ArgumentTypes.Select(a => a.CanBeNull));
        }

        // Если любой параметр прокидывает null и может быть null.
        for (int i = 0; i < function.Arguments.Count; i++)
        {
            if (function.Arguments[i].PropagateNull && sign.ArgumentTypes[i].CanBeNull)
            {
                return true;
            }
        }

        return false;
    }
}