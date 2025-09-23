using Pattern.Unions;
using ReData.Query.Common;
using ReData.Query.Core.Types;

namespace ReData.Query.Core;

public static class QueryBuilderExtensions
{
    public static Result<QueryBuilder, IEnumerable<ExprError[]>> Where(
        this Result<QueryBuilder, IEnumerable<ExprError[]>> qb, string condition)
    {
        if (qb.IsOk(out var ok))
        {
            return ok.Where(condition);
        }

        return qb;
    }

    public static Result<QueryBuilder, IEnumerable<ExprError[]>> OrderBy(
        this Result<QueryBuilder, IEnumerable<ExprError[]>> qb, IReadOnlyList<(string, OrderItem.Type)> orderings)
    {
        if (qb.IsOk(out var ok))
        {
            return ok.OrderBy(orderings);
        }

        return qb;
    }

    public static Result<QueryBuilder, IEnumerable<ExprError[]>> Select(
        this Result<QueryBuilder, IEnumerable<ExprError[]>> qb, Dictionary<string, string> select)
    {
        if (qb.IsOk(out var ok))
        {
            return ok.Select(select);
        }

        return qb;
    }

    public static Result<QueryBuilder, IEnumerable<ExprError[]>> GroupBy(
        this Result<QueryBuilder, IEnumerable<ExprError[]>> qb,
        IReadOnlyList<string> groupBy,
        Dictionary<string, string> select)
    {
        if (qb.IsOk(out var ok))
        {
            return ok.GroupBy(groupBy, select);
        }

        return qb;
    }


    public static IReadOnlyList<T> GetErrors<T, E>(
        this IEnumerable<Result<T, E>> results,
        out IEnumerable<E> errors
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
                errors = results.Select(r => r.UnwrapErrOrDefault()!);
                return [];
            }
        }

        errors = Array.Empty<E>();
        return res;
    }
}