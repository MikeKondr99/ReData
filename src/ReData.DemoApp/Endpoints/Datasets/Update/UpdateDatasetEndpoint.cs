using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using ReData.DemoApp.Commands;
using ReData.DemoApp.Database;
using ReData.DemoApp.Database.Entities;
using ReData.DemoApp.Endpoints.Groups;
using Z.EntityFramework.Plus;

namespace ReData.DemoApp.Endpoints.Datasets.Update;

/// <summary>
/// Изменить набор данных
/// </summary>
/// <remarks>
/// Редактирует набор данных с указанным id
/// </remarks>
public class UpdateDatasetEndpoint : Endpoint<UpdateDataSetRequest, Results<Ok<UpdateDataSetResponse>, NotFound>>
{
    public required ApplicationDatabaseContext Db { get; init; }

    public required IOutputCacheStore OutputCache { get; init; }

    public override void Configure()
    {
        Put("/{Id}");
        Group<DataSetsGroup>();
        AllowAnonymous();
    }

    public override async Task<Results<Ok<UpdateDataSetResponse>, NotFound>> ExecuteAsync(
        UpdateDataSetRequest req, CancellationToken ct)
    {
        var exists = await Db.Set<DataSetEntity>()
            .AsNoTracking()
            .AnyAsync(ds => ds.Id == req.Id, ct);
        if (!exists)
        {
            return TypedResults.NotFound();
        }

        var transformations = new List<TransformationEntity>(req.Transformations.Count);
        for (int i = 0; i < req.Transformations.Count; i++)
        {
            transformations.Add(new TransformationEntity
            {
                Enabled = req.Transformations[i].Enabled,
                Description = null,
                DataSetId = req.Id,
                Order = (uint)i,
                Data = req.Transformations[i].Transformation,
            });
        }

        var updatedAt = DateTimeOffset.UtcNow;

        // Добавляем метадату при сохранении набора
        var metadata = await new GetMetadataCommand()
        {
            Transformations = req.Transformations
                .Where(tb => tb.Enabled)
                .Select(tb => tb.Transformation)
                .ToArray(),
            ConnectorId = req.ConnectorId
        }.ExecuteAsync(ct);

        await using var tx = await Db.Database.BeginTransactionAsync(ct);

        await Db.Set<DataSetEntity>()
            .Where(ds => ds.Id == req.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(ds => ds.Name, req.Name)
                .SetProperty(ds => ds.DataConnectorId, req.ConnectorId)
                .SetProperty(ds => ds.RowsCount, metadata.RowsCount)
                .SetProperty(ds => ds.FieldList, metadata.FieldList)
                .SetProperty(ds => ds.UpdatedAt, updatedAt), ct);

        await Db.Set<TransformationEntity>()
            .Where(t => t.DataSetId == req.Id)
            .DeleteAsync(ct);

        if (transformations.Count > 0)
        {
            Db.Set<TransformationEntity>().AddRange(transformations);
            await Db.SaveChangesAsync(ct);
        }

        await tx.CommitAsync(ct);

        var response = new UpdateDataSetResponse
        {
            Id = req.Id,
            Name = req.Name,
            DataConnectorId = req.ConnectorId,
            Transformations = transformations
                .OrderBy(t => t.Order)
                .Select(t => new TransformationBlockResponse
                {
                    Enabled = t.Enabled,
                    Description = t.Description,
                    Transformation = t.Data
                })
                .ToList()
        };

        await OutputCache.EvictByTagAsync("datasets", ct);
        return TypedResults.Ok(response);
    }
}
