using Pattern.Unions;
using ReData.Query.Common;
using ReData.Query.Core;
using ReData.Query.Core.Types;
using ReData.Query.Lang.Expressions;
using QueryOrderItem = ReData.Query.Core.Types.OrderItem;

namespace ReData.DemoApp.Transformations;

public sealed class TransformationHandler
{
    public Result<QueryBuilder, IEnumerable<IReadOnlyList<ExprError>>> Apply(
        QueryBuilder builder,
        TransformationData transformation)
    {
        return transformation switch
        {
            SelectTransformationData select => ApplySelect(builder, select),
            WhereTransformationData where => ApplyWhere(builder, where),
            OrderByTransformationData orderBy => ApplyOrderBy(builder, orderBy),
            LimitOffsetTransformationData limitOffset => ApplyLimitOffset(builder, limitOffset),
            GroupByTransformationData groupBy => ApplyGroupBy(builder, groupBy),
            AggInfoTransformationData aggInfo => ApplyAggInfo(builder, aggInfo),
            _ => throw new NotSupportedException($"Unsupported transformation type '{transformation.GetType().Name}'.")
        };
    }

    private static Result<QueryBuilder, IEnumerable<IReadOnlyList<ExprError>>> ApplySelect(
        QueryBuilder builder,
        SelectTransformationData transformation)
    {
        var select = new Dictionary<string, string>();

        Field[] oldFields = [];
        if (transformation.RestOptions == SelectRestOptions.NoAction)
        {
            var newFields = transformation.Items.Select(i => i.Field).ToHashSet();

            oldFields = builder.Build().Fields().Where(f => !newFields.Contains(f.Alias)).ToArray();
            foreach (var field in oldFields)
            {
                select.Add(
                    field.Alias,
                    field.Type.Type == DataType.Bool
                        ? $"Int({Expr.Field(field.Alias)})"
                        : Expr.Field(field.Alias));
            }
        }

        foreach (var newField in transformation.Items)
        {
            select.Add(newField.Field, newField.Expression);
        }

        return builder.Select(select).MapError(err => err.Skip(oldFields.Length));
    }

    private static Result<QueryBuilder, IEnumerable<IReadOnlyList<ExprError>>> ApplyWhere(
        QueryBuilder builder,
        WhereTransformationData transformation)
    {
        return builder.Where(transformation.Condition);
    }

    private static Result<QueryBuilder, IEnumerable<IReadOnlyList<ExprError>>> ApplyOrderBy(
        QueryBuilder builder,
        OrderByTransformationData transformation)
    {
        var items = transformation.Items
            .Select(i => (i.Expression, i.Descending ? QueryOrderItem.Type.Desc : QueryOrderItem.Type.Asc))
            .ToArray();

        return builder.OrderBy(items);
    }

    private static Result<QueryBuilder, IEnumerable<IReadOnlyList<ExprError>>> ApplyLimitOffset(
        QueryBuilder builder,
        LimitOffsetTransformationData transformation)
    {
        if (transformation.Offset.HasValue)
        {
            builder = builder.Skip(transformation.Offset.Value);
        }

        if (transformation.Limit.HasValue)
        {
            builder = builder.Take(transformation.Limit.Value);
        }

        return builder;
    }

    private static Result<QueryBuilder, IEnumerable<IReadOnlyList<ExprError>>> ApplyGroupBy(
        QueryBuilder builder,
        GroupByTransformationData transformation)
    {
        var select = transformation.Groups
            .Concat(transformation.Items)
            .ToDictionary(item => item.Field, item => item.Expression);
        var groups = transformation.Groups.Select(item => item.Expression).ToArray();

        return builder.GroupBy(groups, select);
    }

    private static Result<QueryBuilder, IEnumerable<IReadOnlyList<ExprError>>> ApplyAggInfo(
        QueryBuilder builder,
        AggInfoTransformationData _)
    {
        var fields = builder.Build().Fields();
        Dictionary<string, string> select = new();

        foreach (var field in fields)
        {
            var alias = field.Alias;
            select[$"{alias}-Density"] = $"SUM(IF([{alias}.NotNull(), 1, 0)) / Num(Count())";
            select[$"{alias}-Count"] = $"COUNT([{alias}])";
            select[$"{alias}-UniqueValues"] = $"COUNT_DISTINCT([{alias}])";
            select[$"{alias}-Min"] = $"MIN([{alias}])";
            select[$"{alias}-Max"] = $"MAX([{alias}])";
            if (field.Type.Type == DataType.Text)
            {
                select[$"{alias}-MinLength"] = $"MIN(Len([{alias}]))";
                select[$"{alias}-MaxLength"] = $"MAX(Len([{alias}]))";
            }
        }

        return builder.Select(select);
    }
}
