using Pattern.Core;
using Pattern.Unions;
using ReData.Query.Common;
using ReData.Query.Core.Components;
using ReData.Query.Core.Components.Implementation;
using ReData.Query.Core.Template;
using ReData.Query.Core.Types;
using ReData.Query.Core.Value;
using ReData.Query.Lang.Expressions;
using ReData.Query.Runners.Value;

namespace ReData.Query.Core;

using ResolutionResult = Result<ResolvedExpr, IReadOnlyList<ExprError>>;

public record QueryBuilder
{
    public IFunctionStorage Functions { get; set; }
    private Query Query { get; init; }
    private ExpressionResolver Resolver { get; init; }
    private IReadOnlyDictionary<string, QueryVariable> Variables { get; init; } =
        new Dictionary<string, QueryVariable>();
    private IVariableRuntime VariableRuntime { get; init; } = DisabledVariableRuntime.Instance;
    private IEnumerable<Field> Fields => Query.Fields();

    public QueryBuilder(Query query, ExpressionResolver resolver, IFunctionStorage functions, IVariableRuntime? variableRuntime = null)
    {
        Query = query;
        Resolver = resolver;
        Functions = functions;
        VariableRuntime = variableRuntime ?? DisabledVariableRuntime.Instance;
    }

    public static QueryBuilder FromDual(ExpressionResolver resolver, IFunctionStorage functions, IVariableRuntime? variableRuntime = null)
    {
        return new QueryBuilder(new Query()
            {
                Name = resolver.ResolveName(["DualQuery"]),
            },
            resolver,
            functions,
            variableRuntime
        );
    }
    
    public static QueryBuilder FromTable(
        ExpressionResolver resolver,
        IFunctionStorage functions,
        ReadOnlySpan<string> path,
        IReadOnlyList<(string name, string column, FieldType type)> fields,
        IVariableRuntime? variableRuntime = null)
    {
        var queryName = "TableQuery";
        return new QueryBuilder(new Query()
        {
            Name = resolver.ResolveName([queryName]),
            From = new TableQuerySource(resolver.ResolveName(path), fields.Select(f => new Field
                {
                    Alias = f.name,
                    Template = resolver.ResolveName([f.column]).Template,
                    Type = f.type,
                }).ToArray()
            ),
            Select = fields.Select((f,i) =>
            {
                return new SelectItem()
                {
                    Alias = f.name,
                    Column = resolver.ResolveName([f.column]),
                    ResolvedExpr = new ResolvedExpr()
                    {
                        Expression = new NameExpr(f.name),
                        Template = resolver.ResolveName([f.column]).Template,
                        Arguments = null,
                        Type = new ExprType()
                        {
                            DataType = f.type.Type,
                            CanBeNull = f.type.CanBeNull,
                            IsConstant = false,
                            Aggregated = false,
                        },
                    }
                };
            }).ToArray()
        },
        resolver,
        functions,
        variableRuntime);
    }

    public Result<QueryBuilder, IEnumerable<IReadOnlyList<ExprError>>> Select(Dictionary<string, string> select)
    {
        var qb = this;
        // Создаем Cte только если запрос не dual запрос
        // TODO: исправить и вернуть оптимизации select
        if (qb.Query.From.Name is not null)
        {
            qb = CreateCte();
        }

        var res = qb.ResolveManySelectItems(
            select,
            r => r.NotBool().NotNull()
        ).GetErrors(out var errors);

        if (res.Count is 0)
        {
            return Result.Error(errors);
        }
        
        var agg = FunctionStorage.AggPropagation(res.Select(m => m.ResolvedExpr.Type));

        if (agg is INone)
        {
            return Result.Error(res.Select(m => (IReadOnlyList<ExprError>)[new ExprError
            {
                Span = m.ResolvedExpr.Expression.Span,
                Message = "Все значения в выборке должны быть либо агрегированными либо нет"
            }
            ])!);

        }
        
        return qb with
        {
            Query = qb.Query with
            {
                Select = res.ToArray(),
            }
        };
    }
    
    public Result<QueryBuilder, IEnumerable<IReadOnlyList<ExprError>>> Where(string condition)
    {
        var qb = this;
        if (Query.Select is not null || Query.Limit > 0 || Query.Offset > 0)
        {
            qb = CreateCte();
        }

        var resolvedScript = qb.ResolveScript(condition);
        if (resolvedScript.UnwrapErr(out var scriptErrors, out var scriptResult))
        {
            return Result.Error(Once.From(scriptErrors).AsEnumerable<IReadOnlyList<ExprError>>());
        }

        ResolutionResult res = scriptResult.Expression;
        res = res.NotAggregated().Bool();

        if (res.UnwrapErr(out var err, out var where))
        {
            return Result.Error(Once.From(err).AsEnumerable<IReadOnlyList<ExprError>>());
        }

        return qb with
        {
            Query = qb.Query with
            {
                Where = [..qb.Query.Where ?? [], where]
            },
            Variables = MergeVariables(qb.Variables, scriptResult.Variables),
        };
    }
    
    public Result<QueryBuilder, IEnumerable<IReadOnlyList<ExprError>>> OrderBy(IReadOnlyList<(string expr, OrderItem.Type type)> order)
    {
        var qb = this;
        if (Query.Select is not null || Query.Limit > 0 || Query.Offset > 0)
        {
            qb = CreateCte();
        }

        IEnumerable<OrderItem> res = qb.ResolveMany(
                order.Select(o => o.expr),
                r => r.NotAggregated().NotBool().NotNull())
            .Select((r, i) => r.Map(e => new OrderItem(e, order[i].type)))
            .GetErrors(out var errors);
        
        if (!res.Any())
        {
            return Result.Error(errors);
        }

        // Игнорируем константы
        res = res.Where(o => !o.ResolvedExpr.Type.IsConstant);
        
        return qb with
        {
            Query = qb.Query with
            {
                OrderBy = res.ToArray()
            }
        };
    }
    
    public Result<QueryBuilder,IEnumerable<IReadOnlyList<ExprError>>> GroupBy(IReadOnlyList<string> groupBy, Dictionary<string, string> select)
    {
        var qb = this;
        if (Query.Select is not null)
        {
            qb = CreateCte();
        }
        
        var resGroup = qb.ResolveMany(
            groupBy,
            r => r.NotAggregated().NotBool().NotNull().NotConst())
        .GetErrors(out var errors);

        var res = qb.ResolveManySelectItems(
            select,
            r => r.NotBool().NotNull().AggregatedOrGrouped(resGroup)
        ).GetErrors(out var errors2);
        
        if (!resGroup.Any()  || !res.Any())
        {
            errors = !errors.Any() ? Enumerable.Repeat<IReadOnlyList<ExprError>>([], groupBy.Count) : errors;
            return Result.Error(errors.Concat(errors2.Skip(groupBy.Count))); 
        }
        
        return qb with
        {
            Query = qb.Query with
            {
                Select = res,
                GroupBy = resGroup,
            }
        };
    }

    private QueryBuilder CreateCte()
    {
        return this with
        {
            Query = new Core.Query()
            {
                Name = CteName(),
                From = Query,
            },
        };
    }

    private ResolvedTemplate CteName()
    {
        var random = $"CTE-{Guid.NewGuid().ToString("N")[..6]}";
        return Resolver.ResolveName([random]);
    }

    private ResolvedTemplate GetColumnName(string alias, int index)
    {
        return Resolver.ResolveName([$"column{index + 1}"]);
    }

    private SelectItem GetSelectItem(int index, string alias, ResolvedExpr res)
    {
        return new SelectItem(alias, GetColumnName(alias, index), res);
    }
    
    public QueryBuilder Take(uint take)
    {
        if (take == 0)
        {
            return this;
        }
        var limit = Query.Limit > 0 ? Math.Min(Query.Limit, take) : take;

        return this with
        {
            Query = Query with
            {
                Limit = limit,
            }
        };
    }
    
    public QueryBuilder Skip(uint skip)
    {
        if (skip == 0)
        {
            return this;
        }
        var offset = Query.Offset + skip;
        var limit = Query.Limit > 0
            ? (skip >= Query.Limit ? 0 : Query.Limit - skip)
            : 0;

        return this with
        {
            Query = Query with
            {
                Offset = offset,
                Limit = limit,
            }
        };
    }
    
    private Result<ResolvedExpr, IReadOnlyList<ExprError>> Resolve(string expr)
    {
        return ResolveScript(expr).Map(r => r.Expression);
    }

    private Result<ResolvedScriptExpr, IReadOnlyList<ExprError>> ResolveScript(string expr)
    {
        var resScript = Expr.ParseScript(expr);
        if (!resScript.Unwrap(out var script, out var error))
        {
            return Result.Error<IReadOnlyList<ExprError>>([error]);
        }

        var scopedVariables = new Dictionary<string, QueryVariable>()
        {
            ["$user_id"] = QueryVariable.FromValue("$user_id", new TextValue("Demonmiker")),
        };
        foreach (var variable in Variables)
        {
            scopedVariables[variable.Key] = variable.Value;
        }

        var context = new ResolutionContext()
        {
            Errors = [],
            Functions = Functions,
            Variables = scopedVariables,
            VariableRuntime = VariableRuntime,
            QuerySource = Query.From ?? Query,
            VariableQuerySource = Query,
        };
        var res = Resolver.ResolveScript(script, context);
        if (res is not null)
        {
            return res;
        }

        return context.Errors;
    }

    private static IReadOnlyDictionary<string, QueryVariable> MergeVariables(
        IReadOnlyDictionary<string, QueryVariable> globalVariables,
        IReadOnlyDictionary<string, QueryVariable> newVariables)
    {
        var merged = new Dictionary<string, QueryVariable>(globalVariables);
        foreach (var variable in newVariables)
        {
            merged[variable.Key] = variable.Value;
        }

        return merged;
    }

    public Query Build()
    {
        return Query;
    }

    private IEnumerable<ResolutionResult> ResolveMany(
        IEnumerable<string> exprs,
        Func<ResolutionResult,ResolutionResult>? condition = null
        )
    {
        condition ??= (r) => r;
        foreach (var expr in exprs)
        {
            ResolutionResult res = condition(Resolve(expr));
            yield return res;
        }
    }
    private IEnumerable<Result<SelectItem, IReadOnlyList<ExprError>>> ResolveManySelectItems(
        Dictionary<string,string> select,
        Func<ResolutionResult, ResolutionResult>? condition = null
        )
    {
        condition ??= (r) => r;
        int i = 0;
        foreach (var kv in select)
        {
            Result<SelectItem, IReadOnlyList<ExprError>> res = condition(Resolve(kv.Value))
              .Map(r => new SelectItem(kv.Key, Resolver.ResolveName([$"column{i + 1}"]), r));
            yield return res;
            i += 1;
        }
    }
}

