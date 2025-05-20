using Pattern.Unions;
using ReData.Query.Common;
using ReData.Query.Core;

namespace ReData.DemoApplication;

public class SelectTransformation : ITransformation
{
    public required SelectItem[] Items { get; init; }

    public Result<QueryBuilder, IEnumerable<ExprError?>> Apply(QueryBuilder builder, IServiceProvider services)
    {
        return builder.Select(Items.ToDictionary(a => a.Field, a => a.Expression));
    }
}

public sealed record SelectItem
{
    public required string Field { get; init; }
    public required string Expression { get; init; }
}


public sealed class ExtractTransformation : ITransformation
{
    public Result<QueryBuilder, IEnumerable<ExprError?>> Apply(QueryBuilder builder, IServiceProvider services)
    {
        var connectinoService = services.GetRequiredService<ConnectionService>();
        return connectinoService.GetQuery();
    }
    
    
}