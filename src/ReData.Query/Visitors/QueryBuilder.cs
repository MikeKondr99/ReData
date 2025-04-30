using System.Runtime.CompilerServices;
using Pattern.Unions;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Visitors;

public record QueryBuilder
{
    private Query Query { get; init; }
    private ExpressionResolver Resolver { get; init; }
    private IFieldStorage Fields => Query.Fields();

    private QueryBuilder(Query query, ExpressionResolver resolver)
    {
        Query = query;
        Resolver = resolver;
    }

    public static QueryBuilder FromDual(ExpressionResolver resolver)
    {
        return new QueryBuilder(new Query()
        {
            Name = resolver.NameResolver.ResolveTableName(["Query1"]),
        }, resolver);
    }
    
    public static QueryBuilder FromTable(ExpressionResolver resolver, ReadOnlySpan<string> path, IReadOnlyList<(string name, FieldType type)> fields)
    {
        return new QueryBuilder(new Query()
        {
            Name = resolver.NameResolver.ResolveTableName(["Query1"]),
            From = new TableQuerySource(resolver.NameResolver.ResolveTableName(path), fields.Select(f => new Field
                {
                    Alias = f.name,
                    Type = f.type,
                }).ToArray()
            ),
            Select = fields.Select(f =>
            {
                var fieldTemplate = resolver.NameResolver.ResolveFieldName([f.name], f.type);
                return new Query.Map()
                {
                    Alias = f.name,
                    Template = fieldTemplate,
                    Node = new Node()
                    {
                        Expression = new NameExpr(f.name),
                        Template = fieldTemplate.Template,
                        Arguments = null,
                        Type = new ExprType()
                        {
                            Type = f.type.Type,
                            CanBeNull = f.type.CanBeNull,
                            IsConstant = false,
                            Aggregated = false,
                        },
                    }
                };
            }).ToArray()
        }, resolver);
    }

    public Result<QueryBuilder, QueryBuilderError> Select(Dictionary<string, string> select)
    {
        var qb = this;
        if (Query.Select is not null || Query.Where?.Count > 0 || Query.OrderBy?.Count > 0 )
        {
            qb = CreateCte();
        }

        List<Query.Map> oks = new(select.Count);
        QueryBuilderError errors = default;

        foreach (var m in select)
        {
            if (Resolve(m.Value).Unwrap(out var ok, out var err))
            {
                oks.Add(new Query.Map(m.Key, Resolver.NameResolver.ResolveFieldName([m.Key], new FieldType()), ok));
            }
            else
            {
                errors[m.Key] = MapError(err);
            }
        }

        if (errors.Errors.Count != 0)
        {
            return errors;
        }
        
        return qb with
        {
            Query = qb.Query with
            {
                Select = oks
            }
        };
    }
    
    public Result<QueryBuilder, QueryBuilderError> Where(string condition)
    {
        var qb = this;
        if (Query.Select is not null || Query.Limit > 0 || Query.Offset > 0)
        {
            qb = CreateCte();
        }

        if (!Resolve(condition).Unwrap(out var where, out var error))
        {
            return new QueryBuilderError()
            {
                ["condition"] = MapError(error),
            };
        }

        return qb with
        {
            Query = qb.Query with
            {
                Where = [..Query.Where ?? [], where]
            }
        };
    }
    
    public Result<QueryBuilder,QueryBuilderError> OrderBy(IReadOnlyList<(string expr, Query.Order.Type type)> orderings)
    {
        var qb = this;
        if (Query.Select is not null || Query.Limit > 0 || Query.Offset > 0)
        {
            qb = CreateCte();
        }
        return qb with
        {
            Query = Query with
            {
                // OrderBy = exprs.Select(o => new Query.Order(Resolve(o.expr), o.type)).ToArray(),
            }
        };
    }

    private QueryBuilder CreateCte()
    {
        return this with
        {
            Query = new Query()
            {
                Name = CteName(),
                From = Query,
            }
        };
    }

    private IResolvedTemplate CteName()
    {
        var random = $"CTE-{Guid.NewGuid().ToString("N")[..6]}";
        return Resolver.NameResolver.ResolveTableName([random]);
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
        return this;
    }

    private Result<Node,ResolutionError> Resolve(string expr)
    {
        var resExpr = Expr.Parse(expr);
        if (!resExpr.Unwrap(out var expression, out var error))
        {
            return new ResolutionError.Parsing(error);
        }
        return Resolver.ResolveExpr(expression, Fields);
    }

    private string MapError(ResolutionError error)
    {
        return error switch
        {
            ResolutionError.FieldNotFound(var name, var sug) =>
                $"Поле {name} не распознано, возможно вы имели ввиду {sug}",
            ResolutionError.FunctionNotFound(FunctionResolutionError.FunctionNameNotFound(var name, var sug)) =>
                $"Функция '{name} не найдена, возможно вы имели в виду {sug}'",
            ResolutionError.FunctionNotFound(FunctionResolutionError.FunctionIsNotMethod(var name)) =>
                $"Функция '{name} не может быть вызвана как метод",
            ResolutionError.FunctionNotFound(FunctionResolutionError.FunctionSignatureNotFound(var sign, var sug)) =>
                $"Функция '{sign} не найдена попробуйте:\n {String.Join('\n', sug)}",
            ResolutionError.Parsing(ParsingError.UnexpectedToken err) => $"{err.Message}",
            ResolutionError.Parsing(ParsingError.UnrecognizedSymbol err) => $"{err.Message}",
            _ => throw new ArgumentOutOfRangeException(nameof(error))
        };
    }

    public Query Build()
    {
        return Query;
    }
}

public record struct QueryBuilderError()
{
    public Dictionary<string, string> Errors { get; init; } = new();
    
    public string this[string key]
    {
        get => Errors[key];
        set => Errors[key] = value;
    }
}


public static class QueryBuilderExtensions 
{
    public static Result<QueryBuilder, QueryBuilderError> Where(this Result<QueryBuilder, QueryBuilderError> qb, string condition)
    {
        if (qb.Unwrap(out var ok, out var err))
        {
            return ok.Where(condition);
        }
        return err;
    }
    
    public static Result<QueryBuilder, QueryBuilderError> OrderBy(this Result<QueryBuilder, QueryBuilderError> qb, IReadOnlyList<(string, Query.Order.Type)> orderings)
    {
        if (qb.Unwrap(out var ok, out var err))
        {
            return ok.OrderBy(orderings);
        }
        return err;
    }
    
    public static Result<QueryBuilder, QueryBuilderError> Select(this Result<QueryBuilder, QueryBuilderError> qb, Dictionary<string, string> select)
    {
        if (qb.Unwrap(out var ok, out var err))
        {
            return ok.Select(select);
        }
        return err;
    }
}