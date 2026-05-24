namespace ReData.DemoApp.Transformations;

/// <summary>
/// $type = "limit"
/// </summary>
public sealed record LimitOffsetTransformationData : TransformationData
{
    public uint? Limit { get; set; }
    public uint? Offset { get; set; }
}
