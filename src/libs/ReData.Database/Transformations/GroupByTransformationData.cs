namespace ReData.DemoApp.Transformations;

/// <summary>
/// $type = "groupBy"
/// </summary>
public sealed record GroupByTransformationData : TransformationData
{
    public required SelectItemData[] Groups { get; init; }
    public required SelectItemData[] Items { get; init; }
}
