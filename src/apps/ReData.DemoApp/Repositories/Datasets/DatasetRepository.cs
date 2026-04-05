using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReData.DemoApp.Commands;
using ReData.DemoApp.Database;
using ReData.DemoApp.Database.Entities;
using ReData.DemoApp.Endpoints.Datasets.GetAll;
using ReData.DemoApp.Extensions;
using ReData.DemoApp.Logging;
using ReData.DemoApp.Transformations;
using StrictId;

namespace ReData.DemoApp.Repositories.Datasets;

public sealed class DatasetRepository(ApplicationDatabaseContext db, ILogger<DatasetRepository> logger) : IDatasetRepository
{
    public async Task<List<DataSetListItem>> GetAllAsync(CancellationToken ct)
    {
        using var activity = Tracing.ReData.StartActivity("DatasetRepository.GetAllAsync");

        return await db.DataSets
            .OrderByDescending(ds => ds.UpdatedAt)
            .Select(ds => new DataSetListItem
            {
                Id = ds.Id.ToGuid(),
                Name = ds.Name,
                CreatedAt = ds.CreatedAt,
                UpdatedAt = ds.UpdatedAt,
                FieldList = ds.FieldList,
                RowsCount = ds.RowsCount,
            })
            .ToListAsync(ct);
    }

    public async Task<DataSetEntity?> GetByIdAsync(Id<DataSetEntity> id, CancellationToken ct)
    {
        using var activity = Tracing.ReData.StartActivity("DatasetRepository.GetByIdAsync");

        var entity = await db.Set<DataSetEntity>()
            .Include(ds => ds.Transformations)
            .Where(ds => ds.Id == id)
            .FirstOrDefaultAsync(ct);

        logger.DatasetLoadedById(id, entity is not null);
        return entity;
    }

    public async Task<DataSetEntity?> GetByNameAsync(string name, CancellationToken ct)
    {
        using var activity = Tracing.ReData.StartActivity("DatasetRepository.GetByNameAsync");

        return await db.DataSets
            .Where(ds => ds.Name == name)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<DataSetEntity> CreateAsync(CreateDatasetData data, CancellationToken ct)
    {
        using var activity = Tracing.ReData.StartActivity("DatasetRepository.CreateAsync");

        var datasetId = Id<DataSetEntity>.NewId();
        var now = DateTimeOffset.UtcNow;
        var metadata = await BuildMetadataAsync(data.ConnectorId, data.Transformations, ct);

        var entity = new DataSetEntity
        {
            Id = datasetId,
            Name = data.Name,
            DataConnectorId = data.ConnectorId,
            Transformations = MapTransformations(datasetId, data.Transformations),
            RowsCount = metadata.RowsCount,
            FieldList = metadata.FieldList,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.DataSets.Add(entity);
        await db.SaveChangesAsync(ct);

        logger.DatasetCreated(entity.Id, entity.Name, entity.Transformations.Count);

        await new DatasetChangedEvent
        {
            DatasetId = entity.Id,
            MutationType = DatasetMutationType.Created,
            OccurredAt = now,
            AffectedNames = [entity.Name],
        }.PublishAsync(Mode.WaitForAll, ct);

        return entity;
    }

    public async Task<bool> UpdateAsync(UpdateDatasetData data, CancellationToken ct)
    {
        using var activity = Tracing.ReData.StartActivity("DatasetRepository.UpdateAsync");
        logger.DatasetUpdateStarted(data.Id, data.Name, data.ConnectorId, data.Transformations.Count);

        var current = await db.Set<DataSetEntity>()
            .Where(ds => ds.Id == data.Id)
            .Select(ds => new { ds.Id, ds.Name })
            .FirstOrDefaultAsync(ct);
        if (current is null)
        {
            logger.DatasetUpdateSkippedNotFound(data.Id);
            return false;
        }

        var updatedAt = DateTimeOffset.UtcNow;
        var metadata = await BuildMetadataAsync(data.ConnectorId, data.Transformations, ct);
        var mappedTransformations = MapTransformations(data.Id, data.Transformations);

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        await db.Set<DataSetEntity>()
            .Where(ds => ds.Id == data.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(ds => ds.Name, data.Name)
                .SetProperty(ds => ds.DataConnectorId, data.ConnectorId)
                .SetProperty(ds => ds.RowsCount, metadata.RowsCount)
                .SetProperty(ds => ds.FieldList, metadata.FieldList)
                .SetProperty(ds => ds.UpdatedAt, updatedAt), ct);

        await db.Set<TransformationEntity>()
            .Where(t => t.DataSetId == data.Id)
            .ExecuteDeleteAsync(ct);

        if (mappedTransformations.Count > 0)
        {
            db.Set<TransformationEntity>().AddRange(mappedTransformations);
            await db.SaveChangesAsync(ct);
        }

        await tx.CommitAsync(ct);

        logger.DatasetUpdated(data.Id, current.Name, data.Name, mappedTransformations.Count);

        await new DatasetChangedEvent
        {
            DatasetId = data.Id,
            MutationType = DatasetMutationType.Updated,
            OccurredAt = updatedAt,
            AffectedNames = current.Name == data.Name
                ? [data.Name]
                : [current.Name, data.Name],
        }.PublishAsync(Mode.WaitForAll, ct);

        return true;
    }

    public async Task<bool> DeleteAsync(Id<DataSetEntity> id, CancellationToken ct)
    {
        using var activity = Tracing.ReData.StartActivity("DatasetRepository.DeleteAsync");

        var currentName = await db.DataSets
            .Where(ds => ds.Id == id)
            .Select(ds => ds.Name)
            .FirstOrDefaultAsync(ct);
        if (currentName is null)
        {
            return false;
        }

        var rows = await db.DataSets
            .Where(ds => ds.Id == id)
            .ExecuteDeleteAsync(ct);
        if (rows == 0)
        {
            return false;
        }

        logger.DatasetDeleted(id, currentName);

        await new DatasetChangedEvent
        {
            DatasetId = id,
            MutationType = DatasetMutationType.Deleted,
            OccurredAt = DateTimeOffset.UtcNow,
            AffectedNames = [currentName],
        }.PublishAsync(Mode.WaitForAll, ct);

        return true;
    }

    private static List<TransformationEntity> MapTransformations(
        Id<DataSetEntity> dataSetId,
        IReadOnlyList<TransformationBlock> transformations)
    {
        var result = new List<TransformationEntity>(transformations.Count);
        for (int i = 0; i < transformations.Count; i++)
        {
            result.Add(new TransformationEntity
            {
                Enabled = transformations[i].Enabled,
                Description = null,
                DataSetId = dataSetId,
                Order = (uint)i,
                Data = transformations[i].Transformation,
            });
        }

        return result;
    }

    private static Task<Metadata> BuildMetadataAsync(
        Guid connectorId,
        IReadOnlyList<TransformationBlock> transformations,
        CancellationToken ct)
    {
        return new GetMetadataCommand
        {
            Transformations = transformations
                .Where(tb => tb.Enabled)
                .Select(tb => tb.Transformation)
                .ToArray(),
            ConnectorId = connectorId,
        }.ExecuteAsync(ct);
    }
}

