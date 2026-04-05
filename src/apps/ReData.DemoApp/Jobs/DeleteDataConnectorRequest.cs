namespace ReData.DemoApp.Jobs;

public sealed record DeleteDataConnectorRequest
{
    public required string DataConnectorName { get; init; }
}