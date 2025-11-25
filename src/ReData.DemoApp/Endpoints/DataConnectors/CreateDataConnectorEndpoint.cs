using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using MiniExcelLibs;
using MiniExcelLibs.Csv;
using Npgsql;
using ReData.Common;
using ReData.DemoApp.Commands;
using ReData.DemoApp.Database;
using ReData.DemoApp.Database.Entities;
using ReData.DemoApp.Endpoints.DataSets;
using ReData.DemoApp.Services;
using ReData.Query.Core.Types;
using Entities_Field = ReData.DemoApp.Database.Entities.Field;
using Field = ReData.DemoApp.Database.Entities.Field;

namespace ReData.DemoApp.Endpoints.DataSources;

public class CreateDataConnectorEndpoint : Endpoint<CreateDataConnectorRequest,
    Results<Created<CreateDataConnectorResponse>, BadRequest<string>>>
{
    
    public required IOutputCacheStore OutputCache { get; init; }

    public override void Configure()
    {
        Post("/api/data-connectors");
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
            Name = Query<string>("name"),
            Separator = Query<char>("separator"),
            WithHeader = Query<bool>("withHeader"),
            FileStream = fileStream,
        }.ExecuteAsync(ct);

        var response = new CreateDataConnectorResponse()
        {
            Id = entity.Id,
            Name = entity.Name,
        };

        await OutputCache.EvictByTagAsync("data-connectors", ct);
        
        return TypedResults.Created("lol", response);
    }

}