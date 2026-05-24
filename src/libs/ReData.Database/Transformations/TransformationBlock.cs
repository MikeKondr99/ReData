namespace ReData.DemoApp.Transformations;

public record TransformationBlock
{
    public required bool Enabled { get; init; }
    
    // public required string? Description { get; init; }
    public required TransformationData Transformation { get; init; }

}
