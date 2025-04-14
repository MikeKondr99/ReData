using ReData.DemoApplication;
using ReData.Query;
using ReData.Query.Impl.Runners;
using ReData.Query.Visitors;

public class TransformRequest
{
    public List<ITransformation> Transformations { get; set; } = new ();
}

public sealed record TransformResponse
{
    public required IReadOnlyList<TransformField> Fields { get; init; }
    
    public required int Total { get; init; }
    
    public required string[] Query { get; init; }
    
    public required IReadOnlyList<Dictionary<string,IValue>> Data { get; init; }
    
}

public sealed record TransformField
{
    public required string Alias { get; init; }

    public required DataType Type { get; init; }
    
    public required bool CanBeNull { get; init; }
}