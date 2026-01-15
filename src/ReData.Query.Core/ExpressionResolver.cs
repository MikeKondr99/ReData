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
using ReData.Query.Core.Value;
using ReData.Query.Lang.Expressions;
using ReData.Query.Runners.Value;

namespace ReData.Query.Core;

public record ResolutionContext
{
    public required IQuerySource QuerySource { get; init; }

    public required List<ExprError> Errors { get; init; }

    public required IFunctionStorage Functions { get; init; }

    public required Dictionary<string, IValue> Variables { get; init; }
}

public sealed class ExpressionResolver
{
    public required ILiteralResolver LiteralResolver { private get; init; }

    public ResolvedExpr? ResolveExpr(Expr expr, ResolutionContext context)
    {
        using var span = Tracing.Source.StartActivity("ResolverExpr");
        span?.SetTag("expression", expr.ToString());

        var result = RecursiveResolveExpr(expr, context);


        if (!result.HasValue)
        {
            span?.SetStatus(ActivityStatusCode.Error);
            span?.SetTag("errors", context.Errors.Select(e => e.Message).JoinBy("\n"));
        }

        return result;
    }

    private ResolvedExpr? RecursiveResolveExpr(Expr expr, ResolutionContext context)
    {
        return expr switch
        {
            Literal literal => ResolveLiteral(literal),
            NameExpr name => ResolveName(name, context),
            FuncExpr func => ResolveFunction(func, context),
        };
    }


    // ReSharper disable once MemberCanBePrivate.Global
    private ResolvedExpr ResolveLiteral(Literal literal)
    {
        return LiteralResolver.Resolve(literal);
    }

    private ResolvedExpr? ResolveName(NameExpr name, ResolutionContext context)
    {
        if (context.Variables.Get(name.Value) is ISome<IValue>(var vr))
        {
            Expr expr = Expr.Parse(vr.ToReDataLiteral()).Unwrap();
            ResolvedExpr rexpr = RecursiveResolveExpr(expr, context)!.Value;

            return rexpr with
            {
                Expression = name,
            };
        }

        var source = context.QuerySource;
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

        context.Errors.Add(new ExprError()
        {
            Span = name.Span,
            Message = $"Поле '{name.Value}' не найдено"
        });
        return null;
    }


    private ResolvedExpr? ResolveFunction(FuncExpr funcExpr, ResolutionContext context)
    {
        var arguments = funcExpr.Arguments.Select(a => RecursiveResolveExpr(a, context)).ToArray();

        var args = arguments.Where(a => a.HasValue).Select(a => a!.Value).ToArray();

        if (args.Length != arguments.Length)
        {
            return null;
        }

        var sign = new FunctionSignature
        {
            Name = funcExpr.Name,
            Kind = MapFunctionKind(funcExpr.Kind),
            ArgumentTypes = args.Select(arg => arg.Type).ToArray()
        };
        var def = context.Functions.ResolveFunction(sign);
        if (def.UnwrapErr(out var err, out var definition))
        {
            context.Errors.Add(
                new ExprError()
                {
                    Span = funcExpr.Span,
                    Message = err.Message,
                });
            return null;
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
        return LiteralResolver.ResolveName(path);
    }
}