using System.Diagnostics;
using Pattern;
using Pattern.Unions;
using ReData.Query.Common;
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

    public ResolvedExpr ResolveExpr(ExprNode exprNode, ExprContext context)
    {
        return exprNode switch
        {
            Literal literal => ResolveLiteral(literal),
            NameExprNode name => ResolveName(name, context),
            FuncExprNode func => ResolveFunction(func, context),
        };
    }

    private ResolvedExpr ResolveLiteral(Literal literal)
    {
        return LiteralResolver.Resolve(literal);
    }

    private static ResolvedExpr ResolveName(NameExprNode name, ExprContext context)
    {
        Option<Field> fieldOption = context.Fields.Get(name.Value);
        if (fieldOption is ISome<Field>(var field))
        {
            return new ResolvedExpr
            {
                Node = name,
                Template = Template.Template.Create($"{context.QuerySource.Name?.Template}.{field.Template}"),
                Type = new ExprType()
                {
                    DataType = field.Type.Type,
                    CanBeNull = field.Type.CanBeNull,
                    IsConstant = false,
                    Aggregated = false,
                },
            };
        }

        context.Errors.Add(
            new ExprError()
            {
                Span = name.Span,
                Message = $"Поле '{name.Value}' не найдено"
            });
        return default;
    }

    private ResolvedExpr ResolveFunction(FuncExprNode funcExprNode, ExprContext context)
    {
        ResolvedExpr[] arguments = funcExprNode.Arguments
            .Select(a => ResolveExpr(a, context))
            .ToArray();

        if (context.HasErrors())
        {
            return default;
        }

        var sign = new FunctionSignature
        {
            Name = funcExprNode.Name,
            Kind = MapFunctionKind(funcExprNode.Kind),
            ArgumentTypes = arguments.Select(arg => arg.Type).ToArray()
        };
        
        Result<FunctionResolution, FunctionResolutionError> def = Functions.ResolveFunction(sign);
        if (def.UnwrapErr(out var err, out var definition))
        {
            context.Errors.Add(
                new ExprError()
                {
                    Span = funcExprNode.Span,
                    Message = err.Message,
                });
            return default;
        }

        var function = definition.Function;

        return new ResolvedExpr()
        {
            Node = funcExprNode,
            Template = function.Template,
            Type = new ExprType()
            {
                DataType = function.ReturnType.DataType,
                CanBeNull = definition.PropagatesNull,
                IsConstant = definition.ReturnsConst,
                Aggregated = definition.ReturnsAggregated,
            },
            Arguments = arguments.Zip(definition.Casts).Select(t => t.First with
            {
                Template = t.Second.Template,
                Arguments = [t.First]
            }).ToArray(),
        };
    }

    /// <summary>
    /// Функция оборачивает аргумент в нявный каст
    /// Пример x:int нужно привести к num тогда x превратится в Num(x)
    /// </summary>
    /// <param name="argument">Выражение</param>
    /// <param name="implicitCast">Неявный каст. Должен быть функцией с одним аргументом</param>
    /// <returns>Выражение с применённым неявным кастом</returns>
    private static ResolvedExpr WrapWithImplicitCast(ResolvedExpr argument, FunctionDefinition implicitCast)
    {
        Debug.Assert(implicitCast.Arguments.Count == 1, "implicitCast must be function with one argument");
        return argument with
        {
            Template = implicitCast.Template,
            Arguments = [argument],
        };
    }

    private static FunctionKind MapFunctionKind(FuncExprKind kind) => kind switch
    {
        FuncExprKind.Binary => FunctionKind.Binary,
        FuncExprKind.Method => FunctionKind.Method,
        FuncExprKind.Unary => FunctionKind.Unary,
        FuncExprKind.Default => FunctionKind.Default,
        var a => (FunctionKind)a,
    };

    public ResolvedTemplate ResolveName(ReadOnlySpan<string> path)
    {
        return NameResolver.ResolveName(path);
    }
}