using Pattern;
using Pattern.Core;
using Pattern.Unions;
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
                    Type = f.type,
                }).ToArray()
            ),
            Select = fields.Select((f,i) =>
            {
                return new Map()
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
        if (Query.Select is not null || Query.Where?.Count > 0 || Query.OrderBy?.Count > 0 )
        {
            qb = CreateCte();
        }

        List<Map> oks = new(select.Count);
        List<ExprError?> errors = [];

        var res = ResolveMany(
            select.Select(o => o.Value),
            r => r.NotBool().NotNull()
        ).Map(o => o.Zip(select)
            .Select(p => new Map(p.Second.Key, Resolver.ResolveName([p.Second.Key]), p.First)));
        
        
        if (res.UnwrapErr(out var err, out var ok))
        {
            return Result.Error(err);
        }

        var agg = FunctionStorage.AggPropagation(ok.Select(m => m.ResolvedExpr.Type));

        if (agg is INone)
        {
            return Result.Error(ok.Select(m => (ExprError?)new ExprError
            {
                Span = m.ResolvedExpr.Expression.Span,
                Message = "Все значения в выборке должны быть либо агрегированными либо нет"
            })!);

        }
        
        return qb with
        {
            Query = qb.Query with
            {
                Select = ok.ToArray(),
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
            return Result.Error((IEnumerable<ExprError?>)Once.From(err));
        }

        return qb with
        {
            Query = qb.Query with
            {
                Where = [..Query.Where ?? [], where]
            }
        };
    }
    
    public Result<QueryBuilder,IEnumerable<ExprError?>> OrderBy(IReadOnlyList<(string expr, Order.Type type)> order)
    {
        var qb = this;
        if (Query.Select is not null || Query.Limit > 0 || Query.Offset > 0)
        {
            qb = CreateCte();
        }

        var res = ResolveMany(order.Select(o => o.expr), r => r.NotAggregated().NotBool().NotNull())
            .Map(o => o.Zip(order).Select(p => new Order(p.First, p.Second.type)));
        
        if (res.UnwrapErr(out var err, out var ord))
        {
            return Result.Error(err);
        }
        
        return qb with
        {
            Query = qb.Query with
            {
                OrderBy = ord.ToArray()
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
    
    public QueryBuilder Take(uint take)
    {
        if (Query.Limit is not null)
        {
            take = Math.Min(Query.Limit.Value, take);
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
        if (limit is not null)
        {
            limit = Math.Min(limit.Value, skip);
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

    private Result<IReadOnlyCollection<ResolvedExpr>, IEnumerable<ExprError?>> ResolveMany(
        IEnumerable<string> exprs,
        Func<Result<ResolvedExpr,ExprError>,Result<ResolvedExpr,ExprError>>? condition = null
        )
    {
        condition ??= (r) => r;
        IEnumerable<Result<ResolvedExpr, ExprError>> iter = exprs.Select(expr => condition(Resolve(expr)));
        var res = iter.ToResult();
        if (res.IsOk(out var ok))
        {
            return Result.Ok(ok);
        }
        return Result.Error(iter.Select(r => r.IsError(out var err) ? err : null));
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
    
    public static Result<QueryBuilder, IEnumerable<ExprError?>> OrderBy(this Result<QueryBuilder, IEnumerable<ExprError?>> qb, IReadOnlyList<(string, Order.Type)> orderings)
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
}

