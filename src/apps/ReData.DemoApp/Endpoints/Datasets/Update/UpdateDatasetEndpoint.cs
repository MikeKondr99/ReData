using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using ReData.DemoApp.Database.Entities;
using ReData.DemoApp.Endpoints.Groups;
using ReData.DemoApp.Repositories.Datasets;
using StrictId;

namespace ReData.DemoApp.Endpoints.Datasets.Update;

/// <summary>
/// Изменить набор данных
/// </summary>
/// <remarks>
/// Редактирует набор данных с указанным id
/// </remarks>
public class UpdateDatasetEndpoint : Endpoint<UpdateDataSetRequest, Results<Ok<UpdateDataSetResponse>, NotFound>>
{
    public required IDatasetRepository Datasets { get; init; }

    public override void Configure()
    {
        Put("/{Id}");
        Group<DataSetsGroup>();
        AllowAnonymous();
    }

    public override async Task<Results<Ok<UpdateDataSetResponse>, NotFound>> ExecuteAsync(
        UpdateDataSetRequest req,
        CancellationToken ct)
    {
        var updated = await Datasets.UpdateAsync(
            new UpdateDatasetData
            {
                Id = new Id<DataSetEntity>(req.Id),
                Name = req.Name,
                ConnectorId = req.ConnectorId,
                Transformations = req.Transformations,
            },
            ct);
        if (!updated)
        {
            return TypedResults.NotFound();
        }

        var response = new UpdateDataSetResponse
        {
            Id = req.Id,
            Name = req.Name,
            DataConnectorId = req.ConnectorId,
            Transformations = req.Transformations
                .Select(tb => new TransformationBlockResponse
                {
                    Enabled = tb.Enabled,
                    Description = null,
                    Transformation = tb.Transformation,
                })
                .ToList(),
        };

        return TypedResults.Ok(response);
    }
}
