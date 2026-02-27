using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ReData.DemoApp.Commands;
using ReData.DemoApp.Database;
using ReData.DemoApp.Database.Entities;
using ReData.DemoApp.Endpoints.Datasets.GetAll;
using ReData.DemoApp.Transformations;
using Z.EntityFramework.Plus;

namespace ReData.DemoApp.Repositories.Datasets;

public sealed class DatasetRepository(ApplicationDatabaseContext db) : IDatasetRepository
{
    private const string DatasetsListTag = "dataset:list";

    private static readonly MemoryCacheEntryOptions QueryCacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
    };

    public async Task<List<DataSetListItem>> GetAllAsync(CancellationToken ct)
    {
        var result = await db.DataSets
            .OrderByDescending(ds => ds.UpdatedAt)
            .Select(ds => new DataSetListItem
            {
                Id = ds.Id,
                Name = ds.Name,
                CreatedAt = ds.CreatedAt,
                UpdatedAt = ds.UpdatedAt,
                FieldList = ds.FieldList,
                RowsCount = ds.RowsCount,
            })
            .FromCacheAsync(QueryCacheOptions, ct, DatasetsListTag);

        return result.ToList();
    }

    public async Task<DataSetEntity?> GetByIdWithTransformationsAsync(Guid id, CancellationToken ct)
    {
        var result = await db.Set<DataSetEntity>()
            .Include(ds => ds.Transformations)
            .Where(ds => ds.Id == id)
            .FromCacheAsync(QueryCacheOptions, ct, GetDatasetTag(id));

        return result.FirstOrDefault();
    }

    public async Task<DataSetEntity> CreateAsync(
        string name,
        Guid connectorId,
        IReadOnlyList<TransformationBlock> transformations,
        CancellationToken ct)
    {
        var datasetId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var metadata = await BuildMetadataAsync(connectorId, transformations, ct);

        var entity = new DataSetEntity
        {
            Id = datasetId,
            Name = name,
            DataConnectorId = connectorId,
            Transformations = MapTransformations(datasetId, transformations),
            RowsCount = metadata.RowsCount,
            FieldList = metadata.FieldList,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.DataSets.Add(entity);
        await db.SaveChangesAsync(ct);

        await new DatasetChangedEvent
        {
            DatasetId = entity.Id,
            MutationType = DatasetMutationType.Created,
            OccurredAt = now,
        }.PublishAsync(Mode.WaitForAll, ct);

        return entity;
    }

    public async Task<bool> UpdateAsync(
        Guid id,
        string name,
        Guid connectorId,
        IReadOnlyList<TransformationBlock> transformations,
        CancellationToken ct)
    {
        var exists = await db.Set<DataSetEntity>()
            .AsNoTracking()
            .AnyAsync(ds => ds.Id == id, ct);
        if (!exists)
        {
            return false;
        }

        var updatedAt = DateTimeOffset.UtcNow;
        var metadata = await BuildMetadataAsync(connectorId, transformations, ct);
        var mappedTransformations = MapTransformations(id, transformations);

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        await db.Set<DataSetEntity>()
            .Where(ds => ds.Id == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(ds => ds.Name, name)
                .SetProperty(ds => ds.DataConnectorId, connectorId)
                .SetProperty(ds => ds.RowsCount, metadata.RowsCount)
                .SetProperty(ds => ds.FieldList, metadata.FieldList)
                .SetProperty(ds => ds.UpdatedAt, updatedAt), ct);

        await db.Set<TransformationEntity>()
            .Where(t => t.DataSetId == id)
            .ExecuteDeleteAsync(ct);

        if (mappedTransformations.Count > 0)
        {
            db.Set<TransformationEntity>().AddRange(mappedTransformations);
            await db.SaveChangesAsync(ct);
        }

        await tx.CommitAsync(ct);

        await new DatasetChangedEvent
        {
            DatasetId = id,
            MutationType = DatasetMutationType.Updated,
            OccurredAt = updatedAt,
        }.PublishAsync(Mode.WaitForAll, ct);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var rows = await db.DataSets
            .Where(ds => ds.Id == id)
            .ExecuteDeleteAsync(ct);
        if (rows == 0)
        {
            return false;
        }

        await new DatasetChangedEvent
        {
            DatasetId = id,
            MutationType = DatasetMutationType.Deleted,
            OccurredAt = DateTimeOffset.UtcNow,
        }.PublishAsync(Mode.WaitForAll, ct);

        return true;
    }

    private static List<TransformationEntity> MapTransformations(
        Guid dataSetId,
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

    private static string GetDatasetTag(Guid id) => $"dataset:{id}";
}
