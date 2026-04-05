using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using ReData.Common;
using ReData.DemoApp.Commands;
using ReData.DemoApp.Endpoints.Groups;

namespace ReData.DemoApp.Endpoints.DataConnectors.Create;

/// <summary>
/// Создать коннектор данных
/// </summary>
/// <remarks>
/// Создаёт коннектор данных из поданного CSV файла и конфигурации
/// </remarks>
public class CreateDataConnectorEndpoint : Endpoint<CreateDataConnectorRequest,
    Results<Created<CreateDataConnectorResponse>, BadRequest<string>>>
{
    
    public required IOutputCacheStore OutputCache { get; init; }

    /// <inheritdoc />
    public override void Configure()
    {
        Post("/");
        Group<DataConnectorsGroup>();
        AllowAnonymous();
        AllowFileUploads(dontAutoBindFormData: true);
        // MaxRequestBodySize(50 * 1024 * 1024);
    }

    public override async Task<Results<Created<CreateDataConnectorResponse>, BadRequest<string>>> ExecuteAsync(
        CreateDataConnectorRequest req, CancellationToken ct)
    {
        Stream? fileStream = (await FormMultipartSectionsAsync(ct).FirstOrDefaultAsync()).FileSection?.Section.Body;
        
        var entity = await new CreateDataConnectorCommand
        {
            Name = Query<string>("name")!,
            Separator = Query<char>("separator"),
            WithHeader = Query<bool>("withHeader"),
            FileStream = fileStream!,
        }.ExecuteAsync(ct);

        var response = new CreateDataConnectorResponse()
        {
            Id = entity.Id,
            Name = entity.Name,
        };

        await OutputCache.EvictByTagAsync("data-connectors", ct);
        
        return TypedResults.Created(entity.Id.ToString(), response);
    }

}