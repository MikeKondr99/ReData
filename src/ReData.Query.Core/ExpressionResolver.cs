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

    public Query? VariableQuerySource { get; init; }

    public required List<ExprError> Errors { get; init; }

    public required IFunctionStorage Functions { get; init; }

    public required Dictionary<string, QueryVariable> Variables { get; init; }

    public required IVariableRuntime VariableRuntime { get; init; }
}

public sealed record ResolvedScriptExpr
{
    public required ResolvedExpr Expression { get; init; }

    public required IReadOnlyDictionary<string, QueryVariable> Variables { get; init; }
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
        // 1) Собираем локальные переменные из script.Variables.
        var localVariables = ResolveLocalVariables(script, context);

        if (context.Errors.Count > 0)
        {
            return null;
        }

        // 2) Формируем область видимости: глобальные + локальные.
        var scopeVariables = MergeVariables(context.Variables, localVariables);

        // 3) Резолвим финальное выражение в локальном скоупе.
        var localContext = context with
        {
            Variables = scopeVariables,
        };

        var resolved = RecursiveResolveExpr(script.Expression, localContext);
        if (!resolved.HasValue)
        {
            return null;
        }

        return new ResolvedScriptExpr
        {
            Expression = resolved.Value,
            Variables = localVariables,
        };
    }

    private Dictionary<string, QueryVariable> ResolveLocalVariables(ExpressionScript script, ResolutionContext context)
    {
        var localVariables = new Dictionary<string, QueryVariable>();
        var scopeVariables = new Dictionary<string, QueryVariable>(context.Variables);

        foreach (var declaration in script.Variables)
        {
            if (scopeVariables.ContainsKey(declaration.Name))
            {
                context.Errors.Add(new ExprError()
                {
                    Span = declaration.Expression.Span,
                    Message = $"Переменная '{declaration.Name}' уже существует"
                });
                continue;
            }

            var scopeContext = context with
            {
                Variables = scopeVariables,
            };

            var declarationContext = scopeContext with
            {
                QuerySource = context.VariableQuerySource ?? context.QuerySource,
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
                    Message = $"Переменная '{declaration.Name}' должна быть константой или агрегацией"
                });
                continue;
            }

            var literalValue = TryGetDirectLiteralValue(declaration.Expression);
            if (literalValue is not null)
            {
                var valueVariable = QueryVariable.FromValue(declaration.Name, literalValue);
                localVariables[declaration.Name] = valueVariable;
                scopeVariables[declaration.Name] = valueVariable;
                continue;
            }

            if (context.VariableQuerySource is null)
            {
                context.Errors.Add(new ExprError()
                {
                    Span = declaration.Expression.Span,
                    Message = $"Переменная '{declaration.Name}' не может быть вычислена в текущем контексте"
                });
                continue;
            }

            var variable = context.VariableRuntime.Create(declaration.Name, context.VariableQuerySource, resolved.Value);
            localVariables[declaration.Name] = variable;
            scopeVariables[declaration.Name] = variable;
        }

        return localVariables;
    }

    private static Dictionary<string, QueryVariable> MergeVariables(
        IReadOnlyDictionary<string, QueryVariable> globalVariables,
        IReadOnlyDictionary<string, QueryVariable> localVariables)
    {
        var merged = new Dictionary<string, QueryVariable>(globalVariables);
        foreach (var local in localVariables)
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
        if (TryResolveVariable(name, context, out var variableResolved))
        {
            return variableResolved;
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

    private bool TryResolveVariable(NameExpr name, ResolutionContext context, out ResolvedExpr? resolved)
    {
        resolved = null;
        if (!context.Variables.TryGetValue(name.Value, out var variable))
        {
            return false;
        }

        using var variableSpan = Tracing.Source.StartActivity("VariableResolve");
        variableSpan?.SetTag("variable.name", name.Value);
        var cachedBeforeResolve = variable.Value is not null;
        variableSpan?.SetTag("variable.cached_before", cachedBeforeResolve);

        var resolvedValue = context.VariableRuntime.Resolve(variable);
        if (resolvedValue.UnwrapErr(out var error, out var value))
        {
            variableSpan?.SetStatus(ActivityStatusCode.Error);
            variableSpan?.SetTag("error", error);
            context.Errors.Add(new ExprError()
            {
                Span = name.Span,
                Message = error,
            });
            return true;
        }

        variableSpan?.SetTag("variable.resolve_mode", cachedBeforeResolve ? "value" : "computed");
        variable.Value = value;
        context.Variables[name.Value] = variable;

        var valueExpr = Expr.Parse(value.ToReDataLiteral()).Unwrap();
        var resolvedExpr = RecursiveResolveExpr(valueExpr, context);
        if (!resolvedExpr.HasValue)
        {
            return true;
        }

        resolved = resolvedExpr.Value with
        {
            Expression = name,
        };

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
        IReadOnlyList<IValue?>? constArguments = null;

        if (function.Arguments.Any(a => a.IsConstRequired))
        {
            constArguments = funcExpr.Arguments.Select(a => TryGetConstValue(a, context)).ToArray();

            for (var i = 0; i < function.Arguments.Count; i++)
            {
                if (function.Arguments[i].IsConstRequired && constArguments[i] is null)
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
            Arguments = constArguments ?? Array.Empty<IValue?>(),
            Variables = context.Variables
                .Where(v => v.Value.Value is not null)
                .ToDictionary(v => v.Key, v => v.Value.Value!),
        };

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

    private static IValue? TryGetConstValue(Expr expr, ResolutionContext context)
    {
        return expr switch
        {
            IntegerLiteral(var v) => new IntegerValue(v),
            NumberLiteral(var v) => new NumberValue(v),
            BooleanLiteral(var v) => new BoolValue(v),
            StringLiteral(var v) => new TextValue(v),
            NullLiteral => default(NullValue),
            NameExpr varName when context.Variables.TryGetValue(varName.Value, out var variable) => variable.Value,
            _ => null
        };
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

}
