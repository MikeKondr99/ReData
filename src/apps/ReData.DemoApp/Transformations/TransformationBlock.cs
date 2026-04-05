namespace ReData.DemoApp.Transformations;

public record TransformationBlock
{
    public required bool Enabled { get; init; }
    
    // public required string? Description { get; init; }
    public required Transformation Transformation { get; init; }

}