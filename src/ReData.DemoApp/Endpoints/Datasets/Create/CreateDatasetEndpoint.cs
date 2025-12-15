using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using ReData.DemoApp.Commands;
using ReData.DemoApp.Database;
using ReData.DemoApp.Database.Entities;
using ReData.DemoApp.Endpoints.Datasets.Create;
using ReData.DemoApp.Transformations;

namespace ReData.DemoApp.Endpoints.Datasets.CreateDataset;

using Response =
    Results<
        Created<CreateDataSetResponse>,
        BadRequest<ErrorResponse>,
        Conflict<ErrorResponse>
    >;

public class CreateDatasetEndpoint : Endpoint<CreateDataSetRequest, Response>
{
    public required ApplicationDatabaseContext Db { get; init; }
    public required IOutputCacheStore OutputCache { get; init; }

    public override void Configure()
    {
        Post("/api/datasets");
        Summary(summary =>
        {
            summary.ExampleRequest = new CreateDataSetRequest
            {
                Name = "Имя набора",
                ConnectorId = Guid.Empty,
                Transformations =
                [
                    new TransformationBlock()
                    {
                        Enabled = true,
                        Transformation = new SelectTransformation()
                        {
                            Items =
                            [
                                new SelectItem()
                                {
                                    Field = "Поле",
                                    Expression = "id * 2",
                                }
                            ],
                            RestOptions = SelectRestOptions.Delete,
                        }
                    }
                ]
            };
        });
        AllowAnonymous();
    }

    public override async Task<Response> ExecuteAsync(
        CreateDataSetRequest req,
        CancellationToken ct)
    {
        var newId = Guid.NewGuid();
        
        var metadata = await new GetMetadataCommand()
        {
            Transformations = req.Transformations
                .Where(tb => tb.Enabled)
                .Select(tb => tb.Transformation)
                .ToArray(),
            ConnectorId = req.ConnectorId,
        }.ExecuteAsync(ct);
        
        // Map to entity
        var entity = new DataSetEntity
        {
            Id = newId,
            Name = req.Name,
            DataConnectorId = req.ConnectorId,
            Transformations = req.Transformations.Select((t, index) => new TransformationEntity
            {
                Enabled = t.Enabled,
                Description = null,
                DataSetId = newId,
                Order = (uint)index,
                Data = t.Transformation
            }).ToList(),
            RowsCount = metadata.RowsCount,
            FieldList = metadata.FieldList,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        Db.DataSets.Add(entity);
        await Db.SaveChangesAsync(ct);

        var response = new CreateDataSetResponse
        {
            Id = entity.Id,
            Name = entity.Name,
        };

        await OutputCache.EvictByTagAsync("datasets", ct);
        return TypedResults.Created($"/api/datasets/{entity.Id}", response);
    }
}