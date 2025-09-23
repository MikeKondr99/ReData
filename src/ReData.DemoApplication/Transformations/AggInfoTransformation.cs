using Pattern.Unions;
using ReData.Query.Common;
using ReData.Query.Core;
using ReData.Query.Core.Types;

namespace ReData.DemoApplication.Transformations;

public class AggInfoTransformation : ITransformation
{
    public Result<QueryBuilder,IEnumerable<ExprError[]>> Apply(QueryBuilder builder)
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
            if (field.Type.Type is DataType.Text)
            {
                select[$"{alias}-MinLength"] = $"MIN(Len([{alias}]))";
                select[$"{alias}-MaxLength"] = $"MAX(Len([{alias}]))";
            }
        }
        return builder.Select(select);
    }
}