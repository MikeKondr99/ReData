namespace ReData.DemoApp.Endpoints.Transform;

public sealed record TransformErrorResponse
{
    public required int Index { get; init; }
    
    public required string Message { get; init; }
    public required object? Errors { get; init; }
}