using System.Globalization;
using Dunet;
using FuzzySharp;
using Pattern.Unions;
using ReData.Core;
using ReData.Query.Functions;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Visitors;

public sealed class ExpressionResolver
{
    public required ILiteralResolver LiteralResolver { get; init; }
    public required INameResolver NameResolver { get; init; }
    public required IFunctionStorage Functions { get; init; }

    public Result<Node, ResolutionError> ResolveExpr(IExpr expr, IFieldStorage fields)
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

    public Result<Node, ResolutionError> ResolveName(NameExpr name, IFieldStorage fields)
    {
        var fieldOption = fields[name.Value];
        if (fieldOption is ISome<Field>(var field))
        {
            return new Node
            {
                Expression = name,
                Template = NameResolver.ResolveFieldName([field.Alias], field.Type).Template,
                Type = new ExprType()
                {
                    Type = field.Type.Type,
                    CanBeNull = field.Type.CanBeNull,
                    IsConstant = false,
                    Aggregated = false,
                },
            };
        }
        
        var suggest = Process.ExtractOne(name.Value, fields.Fields.Select(f => f.Alias).ToArray());
        if (suggest.Score > 80)
        {
            return new ResolutionError.FieldNotFound(name.Value, suggest.Value);
        }
        return new ResolutionError.FieldNotFound(name.Value, null);
    }

    
    public Result<Node, ResolutionError> ResolveFunction(FuncExpr funcExpr, IFieldStorage fields)
    {
        var arguments =
            funcExpr.Arguments.Select(a => ResolveExpr(a, fields)).ToResult();

        if (!arguments.Unwrap(out var args, out var error))
        {
            return error;
        }
        
        var sign = new FunctionSignature
        {
            Name = funcExpr.Name,
            Kind = MapFunctionKind(funcExpr.Kind),
            ArgumentTypes = args.Select(arg => new FunctionArgumentType()
            {
                DataType = arg.Type.Type,
                CanBeNull = arg.Type.CanBeNull,
            }).ToArray()
        };
        var def = Functions.ResolveFunction(sign) ?? throw new Exception($"function `{sign} not found");
        if (!def.Unwrap(out var definition, out var funcResError))
        {
            return new ResolutionError.FunctionNotFound(funcResError);
        }
        
        var function = definition.Function;

        return new Node()
        {
            Expression = funcExpr,
            Template = function.Template,
            Type = new ExprType()
            {
                Type = function.ReturnType.DataType,
                CanBeNull = PropagatesNull(function, sign),
                IsConstant = args.All(arg => arg.Type.IsConstant),
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

[Union]
public partial record ResolutionError
{
    public sealed partial record FieldNotFound(string Name, string? Suggestion);
    public sealed partial record FunctionNotFound(FunctionResolutionError Error);
    public sealed partial record Parsing(ParsingError Error);
}