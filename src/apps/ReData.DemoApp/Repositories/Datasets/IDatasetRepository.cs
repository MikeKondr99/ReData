using ReData.DemoApp.Database.Entities;
using ReData.DemoApp.Endpoints.Datasets.GetAll;
using StrictId;

namespace ReData.DemoApp.Repositories.Datasets;

public interface IDatasetRepository
{
    Task<List<TOut>> GetList<TOut>(CancellationToken ct)
    where TOut : IProjection<DatasetEntity, TOut>;

    Task<DatasetEntity?> GetByIdAsync(Id<DatasetEntity> id, CancellationToken ct);

    Task<DatasetEntity?> GetByNameAsync(string name, CancellationToken ct);

    Task<DatasetEntity> CreateAsync(CreateDatasetData data, CancellationToken ct);

    Task<bool> UpdateAsync(UpdateDatasetData data, CancellationToken ct);

    Task<bool> DeleteAsync(Id<DatasetEntity> id, CancellationToken ct);
}
