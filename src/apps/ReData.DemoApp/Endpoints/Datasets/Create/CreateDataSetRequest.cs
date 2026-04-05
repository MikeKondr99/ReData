using ReData.DemoApp.Transformations;

namespace ReData.DemoApp.Endpoints.Datasets.Create;

/// <summary>
/// Запрос на создание набора данных
/// </summary>
public sealed record CreateDataSetRequest
{
    public required string Name { get; init; }
    
    public required Guid ConnectorId { get; init; }
    public required IReadOnlyList<TransformationBlock> Transformations { get; init; }
}