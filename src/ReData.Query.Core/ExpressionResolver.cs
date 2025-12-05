using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Pattern;
using Pattern.Unions;
using ReData.Common;
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

    public Result<ResolvedExpr, ExprError> ResolveExpr(Expr expr, IQuerySource source)
    {
        using var span = Tracing.Source.StartActivity("expression resolution");
        span?.SetTag("expression", expr.ToString());

        var result = RecursiveResolveExpr(expr, source);
        
        if (result.IsError())
        {
            span?.SetStatus(ActivityStatusCode.Error);
        }

        var resExpr = result.Unwrap();
        
        // очень дорого, но можно включить если надо
        // StringBuilder sql = new StringBuilder();
        // new ExpressionCompiler().Compile(sql, resExpr);
        // span?.SetTag("sql", sql.ToString());

        return result;
    }

    private Result<ResolvedExpr, ExprError> RecursiveResolveExpr(Expr expr, IQuerySource source)
    {
        return expr switch
        {
            Literal literal => ResolveLiteral(literal),
            NameExpr name => ResolveName(name, source),
            FuncExpr func => ResolveFunction(func, source),
        };
    }


    // ReSharper disable once MemberCanBePrivate.Global
    public ResolvedExpr ResolveLiteral(Literal literal)
    {
        return LiteralResolver.Resolve(literal);
    }

    public static Result<ResolvedExpr, ExprError> ResolveName(NameExpr name, IQuerySource source)
    {
        var fieldOption = source.Fields().Get(name.Value);
        if (fieldOption is ISome<Field>(var field))
        {
            return new ResolvedExpr
            {
                Expression = name,
                Template = Template.Template.Create($"{source.Name?.Template}.{field.Template}"),
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
            Message = $"Поле '{name.Value}' не найдено"
        };
    }


    public Result<ResolvedExpr, ExprError> ResolveFunction(FuncExpr funcExpr, IQuerySource source)
    {
        var arguments = funcExpr.Arguments.Select(a => RecursiveResolveExpr(a, source)).ToResult();

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
            Arguments = args.Zip(definition.Casts).Select(t => t.First with
            {
                Template = t.Second.Template,
                Arguments = [t.First]
            }).ToArray(),
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