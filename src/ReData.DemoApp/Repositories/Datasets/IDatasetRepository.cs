using ReData.DemoApp.Database.Entities;
using ReData.DemoApp.Endpoints.Datasets.GetAll;
using ReData.DemoApp.Transformations;

namespace ReData.DemoApp.Repositories.Datasets;

public interface IDatasetRepository
{
    Task<List<DataSetListItem>> GetAllAsync(CancellationToken ct);

    Task<DataSetEntity?> GetByIdWithTransformationsAsync(Guid id, CancellationToken ct);

    Task<DataSetEntity> CreateAsync(
        string name,
        Guid connectorId,
        IReadOnlyList<TransformationBlock> transformations,
        CancellationToken ct);

    Task<bool> UpdateAsync(
        Guid id,
        string name,
        Guid connectorId,
        IReadOnlyList<TransformationBlock> transformations,
        CancellationToken ct);

    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
