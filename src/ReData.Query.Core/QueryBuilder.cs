using Pattern.Core;
using Pattern.Unions;
using ReData.Query.Common;
using ReData.Query.Core.Components;
using ReData.Query.Core.Components.Implementation;
using ReData.Query.Core.Template;
using ReData.Query.Core.Types;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Core;

public record QueryBuilder
{
    private Query Query { get; init; }
    private ExpressionResolver Resolver { get; init; }
    private IFieldStorage Fields => Query.Fields();

    public QueryBuilder(Query query, ExpressionResolver resolver)
    {
        Query = query;
        Resolver = resolver;
    }

    public static QueryBuilder FromDual(ExpressionResolver resolver)
    {
        return new QueryBuilder(new Query()
        {
            Name = resolver.ResolveName(["DualQuery"]),
        }, resolver);
    }
    
    
    public static QueryBuilder FromTable(ExpressionResolver resolver, ReadOnlySpan<string> path, IReadOnlyList<(string name, FieldType type)> fields)
    {
        var queryName = "TableQuery";
        return new QueryBuilder(new Query()
        {
            Name = resolver.ResolveName([queryName]),
            From = new TableQuerySource(resolver.ResolveName(path), fields.Select(f => new Field
                {
                    Alias = f.name,
                    Template = resolver.ResolveName([queryName,f.name]).Template,
                    Type = f.type,
                }).ToArray()
            ),
            Select = fields.Select((f,i) =>
            {
                return new SelectItem()
                {
                    Alias = f.name,
                    Column = resolver.ResolveName([f.name]),
                    ResolvedExpr = new ResolvedExpr()
                    {
                        Expression = new NameExpr(f.name),
                        Template = resolver.ResolveName([f.name]).Template,
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
        }, resolver);
    }

    public Result<QueryBuilder, IEnumerable<ExprError?>> Select(Dictionary<string, string> select)
    {
        var qb = this;
        if (Query.Select is not null)
        {
            qb = CreateCte();
        }

        var res = ResolveManySelectItems(
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
            return Result.Error(res.Select(m => (ExprError?)new ExprError
            {
                Span = m.ResolvedExpr.Expression.Span,
                Message = "Все значения в выборке должны быть либо агрегированными либо нет"
            })!);

        }
        
        return qb with
        {
            Query = qb.Query with
            {
                Select = res.ToArray(),
            }
        };
    }
    
    public Result<QueryBuilder, IEnumerable<ExprError?>> Where(string condition)
    {
        var qb = this;
        if (Query.Select is not null || Query.Limit > 0 || Query.Offset > 0)
        {
            qb = CreateCte();
        }

        var res = Resolve(condition).NotAggregated().Bool();

        if (res.UnwrapErr(out var err, out var where))
        {
            return Result.Error(Once.From(err).AsEnumerable<ExprError?>());
        }

        return qb with
        {
            Query = qb.Query with
            {
                Where = [..qb.Query.Where ?? [], where]
            }
        };
    }
    
    public Result<QueryBuilder,IEnumerable<ExprError?>> OrderBy(IReadOnlyList<(string expr, OrderItem.Type type)> order)
    {
        var qb = this;
        if (Query.Select is not null || Query.Limit > 0 || Query.Offset > 0)
        {
            qb = CreateCte();
        }

        IEnumerable<OrderItem> res = ResolveMany(
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
    
    public Result<QueryBuilder,IEnumerable<ExprError?>> GroupBy(IReadOnlyList<string> groupBy, Dictionary<string, string> select)
    {
        var qb = this;
        if (Query.Select is not null)
        {
            qb = CreateCte();
        }
        
        var resGroup = ResolveMany(
            groupBy,
            r => r.NotAggregated().NotBool().NotNull().NotConst())
        .GetErrors(out var errors);

        var res = ResolveManySelectItems(
            select,
            r => r.NotBool().NotNull().AggregatedOrGrouped(resGroup)
        ).GetErrors(out var errors2);
        
        if (!resGroup.Any()  || !res.Any())
        {
            errors = !errors.Any() ? Enumerable.Repeat<ExprError?>(null, groupBy.Count) : errors;
            return Result.Error(errors.Concat(errors2)); 
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
            }
        };
    }

    private IResolvedTemplate CteName()
    {
        var random = $"CTE-{Guid.NewGuid().ToString("N")[..6]}";
        return Resolver.ResolveName([random]);
    }

    private IResolvedTemplate GetColumnName(string alias, int index)
    {
        return Resolver.ResolveName([$"column{index + 1}"]);
    }

    private SelectItem GetSelectItem(int index, string alias, ResolvedExpr res)
    {
        return new SelectItem(alias, GetColumnName(alias, index), res);
    }
    
    public QueryBuilder Take(uint take)
    {
        if (Query.Limit > 0)
        {
            take = Math.Min(Query.Limit, take);
        }
        return this with
        {
            Query = Query with
            {
                Limit = take
            }
        };
    }
    
    public QueryBuilder Skip(uint skip)
    {
        var offset = skip + Query.Offset;
        var limit = Query.Limit;
        if (limit > 0)
        {
            limit = Math.Min(limit, skip);
        }
        return this with
        {
            Query = Query with
            {
                Offset = offset,
                Limit = limit,
            }
        };
    }

    private Result<ResolvedExpr,ExprError> Resolve(string expr)
    {
        var resExpr = Expr.Parse(expr);
        if (!resExpr.Unwrap(out var expression, out var error))
        {
            return error;
        }
        return Resolver.ResolveExpr(expression, Fields);
    }

    public Query Build()
    {
        return Query;
    }

    private IEnumerable<Result<ResolvedExpr, ExprError>> ResolveMany(
        IEnumerable<string> exprs,
        Func<Result<ResolvedExpr,ExprError>,Result<ResolvedExpr,ExprError>>? condition = null
        )
    {
        condition ??= (r) => r;
        foreach (var expr in exprs)
        {
            Result<ResolvedExpr, ExprError> res = condition(Resolve(expr));
            yield return res;
        }
    }
    private IEnumerable<Result<SelectItem, ExprError>> ResolveManySelectItems(
        Dictionary<string,string> select,
        Func<Result<ResolvedExpr,ExprError>,Result<ResolvedExpr,ExprError>>? condition = null
        )
    {
        condition ??= (r) => r;
        int i = 0;
        foreach (var kv in select)
        {
            Result<SelectItem, ExprError> res = condition(Resolve(kv.Value))
              .Map(r => new SelectItem(kv.Key, Resolver.ResolveName([$"column{i + 1}"]), r));
            yield return res;
            i += 1;
        }
    }
}

public static class QueryBuilderExtensions 
{
    public static Result<QueryBuilder, IEnumerable<ExprError?>> Where(this Result<QueryBuilder, IEnumerable<ExprError?>> qb, string condition)
    {
        if (qb.IsOk(out var ok))
        {
            return ok.Where(condition);
        }
        return qb;
    }
    
    public static Result<QueryBuilder, IEnumerable<ExprError?>> OrderBy(this Result<QueryBuilder, IEnumerable<ExprError?>> qb, IReadOnlyList<(string, OrderItem.Type)> orderings)
    {
        if (qb.IsOk(out var ok))
        {
            return ok.OrderBy(orderings);
        }
        return qb;
    }
    
    public static Result<QueryBuilder, IEnumerable<ExprError?>> Select(this Result<QueryBuilder, IEnumerable<ExprError?>> qb, Dictionary<string, string> select)
    {
        if (qb.IsOk(out var ok))
        {
            return ok.Select(select);
        }
        return qb;
    }

    public static Result<ResolvedExpr, ExprError> NotBool(this Result<ResolvedExpr, ExprError> result)
    {
        return result.And<ResolvedExpr>(expr =>
        {
            if (expr.Type.DataType is DataType.Bool)
            {
                return new ExprError()
                {
                    Span = expr.Expression.Span,
                    Message = "Выражение не может быть булевым"
                };
            }
            return expr;
        });
    }
    
    public static Result<ResolvedExpr, ExprError> NotConst(this Result<ResolvedExpr, ExprError> result)
    {
        return result.And<ResolvedExpr>(expr =>
        {
            if (expr.Type.IsConstant)
            {
                return new ExprError()
                {
                    Span = expr.Expression.Span,
                    Message = "Выражение не может константой"
                };
            }
            return expr;
        });
    }
    
    public static Result<ResolvedExpr, ExprError> NotNull(this Result<ResolvedExpr, ExprError> result)
    {
        return result.And<ResolvedExpr>(expr =>
        {
            if (expr.Type.DataType is DataType.Null)
            {
                return new ExprError()
                {
                    Span = expr.Expression.Span,
                    Message = "Выражение не может быть NULL"
                };
            }
            return expr;
        });
    }
    
    public static Result<ResolvedExpr, ExprError> Bool(this Result<ResolvedExpr, ExprError> result)
    {
        return result.And<ResolvedExpr>(expr =>
        {
            if (expr.Type.DataType is not DataType.Bool)
            {
                return new ExprError()
                {
                    Span = expr.Expression.Span,
                    Message = "Выражение должно быть булевым"
                };
            }
            return expr;
        });
    }
    
    public static Result<ResolvedExpr, ExprError> NotAggregated(this Result<ResolvedExpr, ExprError> result)
    {
        return result.And<ResolvedExpr>(expr =>
        {
            if (expr.Type.Aggregated)
            {
                return new ExprError()
                {
                    Span = expr.Expression.Span,
                    Message = "Выражение не может быть агрегированным"
                };
            }
            return expr;
        });
    }
    
    public static Result<ResolvedExpr, ExprError> AggregatedOrGrouped(this Result<ResolvedExpr, ExprError> result, IEnumerable<ResolvedExpr> grouped)
    {
        return result.And<ResolvedExpr>(expr =>
        {
            if (!expr.Type.Aggregated && !expr.Type.IsConstant && !grouped.Any(g => expr.Expression.Equivalent(g.Expression)))
            {
                return new ExprError()
                {
                    Span = expr.Expression.Span,
                    Message = "Выражение должно быть агрегированным или группированным"
                };
            }
            return expr;
        });
    }
    
    public static IReadOnlyList<T> GetErrors<T,E>(
        this IEnumerable<Result<T,E>> results,
        out IEnumerable<E?> errors
    )
    {
        List<T> res = [];
        foreach (var r in results)
        {
            if (r.Unwrap(out var ok, out var err))
            {
                res.Add(ok);
            }
            else
            {
                errors = results.Select(r => r.UnwrapErrOrDefault());
                return [];
            }
        }
        errors = Array.Empty<E?>();
        return res;
    }
}

