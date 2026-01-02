using FastEndpoints;

namespace ReData.DemoApp.Endpoints.DataSets;

public class CreateDataConnectorRequest
{
    [QueryParam]
    [BindFrom("name")]
    public required string Name { get; set; }

    [QueryParam]
    [BindFrom("separator")]
    public required char Separator { get; set; }

    [QueryParam]
    [BindFrom("withHeader")]
    public required bool WithHeader { get; set; }

    public required IFormFile File { get; init; }
}