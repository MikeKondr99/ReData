using FastEndpoints;

namespace ReData.DemoApplication.Endpoints.DataSources;

// Request DTO
public class CreateDataSourceRequest
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

// Response DTO
public class CreateDataSourceResponse
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
}