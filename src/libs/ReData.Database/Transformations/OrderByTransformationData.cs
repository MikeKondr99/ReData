namespace ReData.DemoApp.Transformations;

/// <summary>
/// $type = "orderBy"
/// </summary>
public sealed record OrderByTransformationData : TransformationData
{
    public required OrderItemData[] Items { get; init; }
}

public record struct OrderItemData
{
    public required string Expression { get; init; }
    public required bool Descending { get; init; }
}
