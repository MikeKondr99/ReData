namespace ReData.DemoApp.Endpoints.Datasets;

/// <summary>
/// 
/// </summary>
public sealed record CreateDataSetResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
}
