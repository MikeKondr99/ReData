namespace ReData.DemoApp.Endpoints.Datasets.GetAll;

public sealed record DataSetListItem
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
}