using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using ReData.DemoApp.Database.Entities;
using ReData.DemoApp.Endpoints.Groups;
using ReData.DemoApp.Repositories.Datasets;
using StrictId;

namespace ReData.DemoApp.Endpoints.Datasets.GetById;

/// <summary>
/// Получить набор данных по id
/// </summary>
/// <remarks>
/// Возвращает набор данных с его трансформациями
/// </remarks>
public class GetDatasetByIdEndpoint : Endpoint<GetDatasetByIdRequest, Results<Ok<DataSetResponse>, NotFound>>
{
    public required IDatasetRepository Datasets { get; init; }

    public override void Configure()
    {
        Get("/{Id}");
        Group<DataSetsGroup>();
        AllowAnonymous();
    }

    public override async Task<Results<Ok<DataSetResponse>, NotFound>> ExecuteAsync(
        GetDatasetByIdRequest req,
        CancellationToken ct)
    {
        var entity = await Datasets.GetByIdAsync(new Id<DataSetEntity>(req.Id), ct);
        if (entity is null)
        {
            return TypedResults.NotFound();
        }

        var response = new DataSetResponse
        {
            Id = entity.Id.ToGuid(),
            Name = entity.Name,
            DataConnectorId = entity.DataConnectorId,
            Transformations = entity.Transformations
                .OrderBy(t => t.Order)
                .Select(t => new TransformationBlockResponse
                {
                    Enabled = t.Enabled,
                    Description = t.Description,
                    Transformation = t.Data,
                })
                .ToList(),
        };

        return TypedResults.Ok(response);
    }
}
