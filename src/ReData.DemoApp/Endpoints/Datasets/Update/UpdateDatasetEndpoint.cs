using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using ReData.DemoApp.Commands;
using ReData.DemoApp.Database;
using ReData.DemoApp.Database.Entities;
using ReData.DemoApp.Endpoints.Groups;

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
        var entity = await Db.Set<DataSetEntity>()
            .AsTracking()
            .Include(ds => ds.Transformations)
            .FirstOrDefaultAsync(ds => ds.Id == req.Id, ct);

        if (entity is null)
        {
            return TypedResults.NotFound();
        }

        // Обновляем простые свойства
        entity.Name = req.Name;
        entity.DataConnectorId = req.ConnectorId;

        // Очищаем и добавляем новые трансформации
        Db.Set<TransformationEntity>().RemoveRange(entity.Transformations);
        entity.Transformations.Clear();
        for (int i = 0; i < req.Transformations.Count; i++)
        {
            entity.Transformations.Add(new TransformationEntity
            {
                Enabled = req.Transformations[i].Enabled,
                Description = null,
                DataSetId = req.Id,
                Order = (uint)i,
                Data = req.Transformations[i].Transformation,
            });
        }

        entity.UpdatedAt = DateTimeOffset.UtcNow;

        var queryBuilder = (await new ApplyTransformationsCommand()
        {
            Transformations = req.Transformations.Where(tb => tb.Enabled).Select(tb => tb.Transformation).ToArray(),
            DataConnectorId = req.ConnectorId
        }.ExecuteAsync(ct)).UnwrapOrDefault();

        // Добавляем метадату при сохранении набора
        var metadata = await new GetMetadataCommand()
        {
            Transformations = req.Transformations
                .Where(tb => tb.Enabled)
                .Select(tb => tb.Transformation)
                .ToArray(),
            ConnectorId = req.ConnectorId
        }.ExecuteAsync(ct);

        entity.RowsCount = metadata.RowsCount;
        entity.FieldList = metadata.FieldList;

        // Сохраняем в БД
        await Db.SaveChangesAsync(ct);

        var response = new UpdateDataSetResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            DataConnectorId = entity.DataConnectorId,
            Transformations = entity.Transformations
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