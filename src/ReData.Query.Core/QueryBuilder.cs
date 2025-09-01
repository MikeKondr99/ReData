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
    private IEnumerable<Field> Fields => Query.Fields();

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
                    Template = resolver.ResolveName([f.name]).Template,
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

        var res = qb.Resolve(condition).NotAggregated().Bool();

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
    
    public Result<QueryBuilder,IEnumerable<ExprError?>> GroupBy(IReadOnlyList<string> groupBy, Dictionary<string, string> select)
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
        return Resolver.ResolveExpr(expression, Query.From);
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

