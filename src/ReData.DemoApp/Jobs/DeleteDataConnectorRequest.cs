namespace ReData.DemoApp.Jobs;

public sealed record DeleteDataConnectorRequest
{
    public string DataConnectorName { get; init; }
}