using FastEndpoints;

namespace ReData.DemoApplication.Endpoints.DataSets;

public class DataConnectorListItem
{
    public Guid Id { get; init; }
    
    public string Name { get; init; }
}

// Request DTO
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

public class CreateDataConnectorResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
}