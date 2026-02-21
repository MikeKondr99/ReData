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

    public Query? ConstantQuerySource { get; init; }

    public required List<ExprError> Errors { get; init; }

    public required IFunctionStorage Functions { get; init; }

    public required Dictionary<string, QueryConstant> Constants { get; init; }

    public required IConstantRuntime ConstantRuntime { get; init; }
}

public sealed record ResolvedScriptExpr
{
    public required ResolvedExpr Expression { get; init; }

    public required IReadOnlyDictionary<string, QueryConstant> Constants { get; init; }
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

    public ResolvedScriptExpr? ResolveScript(ExpressionScript script, ResolutionContext context)
    {
        // 1) Собираем локальные константы из script.Contants.
        var localConstants = ResolveLocalConstants(script, context);

        if (context.Errors.Count > 0)
        {
            return null;
        }

        // 2) Формируем область видимости: глобальные + локальные.
        var scopeConstants = MergeConstants(context.Constants, localConstants);

        // 3) Резолвим финальное выражение в локальном скоупе.
        var localContext = context with
        {
            Constants = scopeConstants,
        };

        var resolved = RecursiveResolveExpr(script.Expression, localContext);
        if (!resolved.HasValue)
        {
            return null;
        }

        return new ResolvedScriptExpr
        {
            Expression = resolved.Value,
            Constants = localConstants,
        };
    }

    private Dictionary<string, QueryConstant> ResolveLocalConstants(ExpressionScript script, ResolutionContext context)
    {
        var localConstants = new Dictionary<string, QueryConstant>();
        var scopeConstants = new Dictionary<string, QueryConstant>(context.Constants);

        foreach (var declaration in script.Contants)
        {
            if (scopeConstants.ContainsKey(declaration.Name))
            {
                context.Errors.Add(new ExprError()
                {
                    Span = declaration.Expression.Span,
                    Message = $"Константа '{declaration.Name}' уже существует"
                });
                continue;
            }

            var scopeContext = context with
            {
                Constants = scopeConstants,
            };

            var declarationContext = scopeContext with
            {
                QuerySource = context.ConstantQuerySource ?? context.QuerySource,
            };

            var resolved = RecursiveResolveExpr(declaration.Expression, declarationContext);
            if (!resolved.HasValue)
            {
                continue;
            }

            if (!resolved.Value.Type.IsConstant && !resolved.Value.Type.Aggregated)
            {
                context.Errors.Add(new ExprError()
                {
                    Span = declaration.Expression.Span,
                    Message = $"Константа '{declaration.Name}' должна быть константой или агрегацией"
                });
                continue;
            }

            var literalValue = TryGetDirectLiteralValue(declaration.Expression);
            if (literalValue is not null)
            {
                var valueConstant = QueryConstant.FromValue(declaration.Name, literalValue);
                localConstants[declaration.Name] = valueConstant;
                scopeConstants[declaration.Name] = valueConstant;
                continue;
            }

            if (context.ConstantQuerySource is null)
            {
                context.Errors.Add(new ExprError()
                {
                    Span = declaration.Expression.Span,
                    Message = $"Константа '{declaration.Name}' не может быть вычислена в текущем контексте"
                });
                continue;
            }

            var constant = context.ConstantRuntime.Create(declaration.Name, context.ConstantQuerySource, resolved.Value);
            localConstants[declaration.Name] = constant;
            scopeConstants[declaration.Name] = constant;
        }

        return localConstants;
    }

    private static Dictionary<string, QueryConstant> MergeConstants(
        IReadOnlyDictionary<string, QueryConstant> globalConstants,
        IReadOnlyDictionary<string, QueryConstant> localConstants)
    {
        var merged = new Dictionary<string, QueryConstant>(globalConstants);
        foreach (var local in localConstants)
        {
            merged[local.Key] = local.Value;
        }

        return merged;
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
        if (TryResolveConstant(name, context, out var constantResolved))
        {
            return constantResolved;
        }

        if (TryResolveField(name, context, out var fieldResolved))
        {
            return fieldResolved;
        }

        context.Errors.Add(new ExprError()
        {
            Span = name.Span,
            Message = $"Поле '{name.Value}' не найдено"
        });
        return null;
    }

    private bool TryResolveConstant(NameExpr name, ResolutionContext context, out ResolvedExpr? resolved)
    {
        resolved = null;
        if (!context.Constants.TryGetValue(name.Value, out var constant))
        {
            return false;
        }

        var resolvedValue = context.ConstantRuntime.Resolve(constant);
        if (resolvedValue.UnwrapErr(out var error, out var value))
        {
            context.Errors.Add(new ExprError()
            {
                Span = name.Span,
                Message = error,
            });
            return true;
        }

        constant.Value = value;
        context.Constants[name.Value] = constant;

        resolved = ResolveValueAsExpr(name, value, context);
        if (!resolved.HasValue)
        {
            return true;
        }

        return true;
    }

    private static bool TryResolveField(NameExpr name, ResolutionContext context, out ResolvedExpr? resolved)
    {
        resolved = null;
        var source = context.QuerySource;
        var fieldOption = source.Fields().Get(name.Value);
        if (fieldOption is not ISome<Field>(var field))
        {
            return false;
        }

        resolved = new ResolvedExpr
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
        return true;
    }


    private ResolvedExpr? ResolveFunction(FuncExpr funcExpr, ResolutionContext context)
    {
        if (funcExpr is { Kind: FuncExprKind.Default, Name: "const" })
        {
            return ResolveInlineConst(funcExpr, context);
        }

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
        if (function.Arguments.Any(a => a.IsConstRequired))
        {
            for (var i = 0; i < function.Arguments.Count; i++)
            {
                if (function.Arguments[i].IsConstRequired && !args[i].Type.IsLiteral)
                {
                    context.Errors.Add(new ExprError()
                    {
                        Span = funcExpr.Arguments[i].Span,
                        Message = $"Функция {function.Name} требует что бы {ArgOrdinal(i)} аргумент был константой"
                    });
                    return null;
                }
            }
        }

        var templateContext = new TemplateContext()
        {
            Fields = context.QuerySource.Fields().ToArray(),
            Arguments = args,
            Constants = context.Constants
                .Where(v => v.Value.Value is not null)
                .ToDictionary(v => v.Key, v => v.Value.Value!),
        };
     
        try
        {
            var template = function.Template.GetTemplate(templateContext);
            return new ResolvedExpr()
            {
                Expression = funcExpr,
                Template = template,
                Type = new ExprType()
                {
                    DataType = function.ReturnType.DataType,
                    CanBeNull = definition.PropagatesNull,
                    IsConstant = definition.ReturnsConst,
                    Aggregated = definition.ReturnsAggregated,
                },
                Arguments = args.Zip(definition.Casts).Select(t => t.First with
                {
                    Template = t.Second.Template.GetTemplate(templateContext),
                    Arguments = [t.First]
                }).ToArray(),
            };
        }
        catch (TemplateExprErrorException ex)
        {
            context.Errors.Add(ex.Error);
            return null;
        }
    }

    private ResolvedExpr? ResolveInlineConst(FuncExpr funcExpr, ResolutionContext context)
    {
        if (funcExpr.Arguments.Count != 1)
        {
            context.Errors.Add(new ExprError()
            {
                Span = funcExpr.Span,
                Message = "Функция const требует ровно один аргумент"
            });
            return null;
        }

        var value = ResolveInlineConstValue(funcExpr, context);
        if (value is null)
        {
            return null;
        }

        return ResolveValueAsExpr(funcExpr, value, context, preserveSourceExpression: false);
    }

    private IValue? ResolveInlineConstValue(FuncExpr funcExpr, ResolutionContext context)
    {
        if (funcExpr.Arguments.Count != 1)
        {
            context.Errors.Add(new ExprError()
            {
                Span = funcExpr.Span,
                Message = "Функция const требует ровно один аргумент"
            });
            return null;
        }

        var literalValue = TryGetDirectLiteralValue(funcExpr.Arguments[0]);
        if (literalValue is not null)
        {
            return literalValue;
        }

        var argument = RecursiveResolveExpr(funcExpr.Arguments[0], context);
        if (!argument.HasValue)
        {
            return null;
        }

        if (!argument.Value.Type.IsConstant && !argument.Value.Type.Aggregated)
        {
            context.Errors.Add(new ExprError()
            {
                Span = funcExpr.Arguments[0].Span,
                Message = "const принимает только константное или агрегатное выражение"
            });
            return null;
        }

        var constantQuery = context.QuerySource as Query ?? context.ConstantQuerySource;
        if (constantQuery is null)
        {
            context.Errors.Add(new ExprError()
            {
                Span = funcExpr.Arguments[0].Span,
                Message = "const не может быть вычислен в текущем контексте"
            });
            return null;
        }

        var constant = context.ConstantRuntime.Create("$const_eval", constantQuery, argument.Value);
        var resolvedValue = context.ConstantRuntime.Resolve(constant);
        if (resolvedValue.UnwrapErr(out var error, out var value))
        {
            context.Errors.Add(new ExprError()
            {
                Span = funcExpr.Arguments[0].Span,
                Message = error,
            });
            return null;
        }

        return value;
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

    private static IValue? TryGetDirectLiteralValue(Expr expr)
    {
        return expr switch
        {
            IntegerLiteral(var v) => new IntegerValue(v),
            NumberLiteral(var v) => new NumberValue(v),
            BooleanLiteral(var v) => new BoolValue(v),
            StringLiteral(var v) => new TextValue(v),
            NullLiteral => default(NullValue),
            _ => null
        };
    }

    private static string ArgOrdinal(int index) => index switch
    {
        0 => "первый",
        1 => "второй",
        2 => "третий",
        3 => "четвертый",
        4 => "пятый",
        _ => $"{index + 1}-й",
    };

    private ResolvedExpr? ResolveValueAsExpr(
        Expr sourceExpr,
        IValue value,
        ResolutionContext context,
        bool preserveSourceExpression = true)
    {
        var literal = value.ToReDataLiteral();
        var parsed = Expr.Parse(literal);
        if (!parsed.Unwrap(out var valueExpr, out var error))
        {
            context.Errors.Add(new ExprError()
            {
                Span = sourceExpr.Span,
                Message = $"Не удалось распарсить вычисленное значение '{literal}': {error.Message}",
            });
            return null;
        }

        var resolvedExpr = RecursiveResolveExpr(valueExpr, context);
        if (!resolvedExpr.HasValue)
        {
            return null;
        }

        return preserveSourceExpression
            ? resolvedExpr.Value with
            {
                Expression = sourceExpr,
            }
            : resolvedExpr.Value;
    }

}
