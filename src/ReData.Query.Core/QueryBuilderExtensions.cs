using Pattern.Unions;
using ReData.Query.Common;
using ReData.Query.Core.Template;
using ReData.Query.Core.Types;

namespace ReData.Query.Core;

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
    
    public static Result<QueryBuilder, IEnumerable<ExprError?>> Select(this Result<QueryBuilder, IEnumerable<ExprError?>> qb, Dictionary<string,string> select)
    {
        if (qb.IsOk(out var ok))
        {
            return ok.Select(select);
        }
        return qb;
    }
    
    public static Result<QueryBuilder, IEnumerable<ExprError?>> GroupBy(this Result<QueryBuilder, IEnumerable<ExprError?>> qb, IReadOnlyList<string> groupBy, Dictionary<string,string> select)
    {
        if (qb.IsOk(out var ok))
        {
            return ok.GroupBy(groupBy, select);
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