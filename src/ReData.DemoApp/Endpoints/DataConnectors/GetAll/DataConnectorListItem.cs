namespace ReData.DemoApp.Endpoints.DataConnectors.GetAll;

/// <summary>
/// Коннектор данных
/// </summary>
public class DataConnectorListItem
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }
}