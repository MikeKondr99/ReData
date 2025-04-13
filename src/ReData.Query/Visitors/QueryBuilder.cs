using System.Runtime.CompilerServices;
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
                    Template = resolver.NameResolver.ResolveFieldName([f.name], f.type).Template,
                }).ToArray()
            ),
        }, resolver).Select(fields.ToDictionary(f => f.name, f => $"[{f.name}]"));
    }

    public QueryBuilder Select(Dictionary<string, string> select)
    {
        if (Query.Select is not null || Query.Where?.Count > 0 || Query.OrderBy?.Count > 0 )
        {
            CreateCte();
        }

        return this with
        {
            Query = Query with
            {
                Select = select.Select(m => new Query.Map(m.Key, Resolver.NameResolver.ResolveFieldName([m.Key],new FieldType()), Resolve(m.Value))).ToArray()
            }
        };
    }
    
    public QueryBuilder Where(string predicate)
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
                Where = [..Query.Where ?? [], Resolve(predicate)]
            }
        };
    }
    
    public QueryBuilder Order(string expr, Query.Order.Type type)
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
                OrderBy = [new Query.Order(Resolve(expr), type)]
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

    private Node Resolve(string expr)
    {
        return Resolver.ResolveExpr(RawExpr.Parse(expr), Fields);
    }

    public Query Build()
    {
        return Query;
    }
    
    

}