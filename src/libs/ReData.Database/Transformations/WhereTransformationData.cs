namespace ReData.DemoApp.Transformations;

/// <summary>
/// $type = "where"
/// </summary>
public sealed record WhereTransformationData : TransformationData
{
    public required string Condition { get; set; }
}
