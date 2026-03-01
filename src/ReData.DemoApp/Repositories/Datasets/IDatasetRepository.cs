using ReData.DemoApp.Database.Entities;
using ReData.DemoApp.Endpoints.Datasets.GetAll;

namespace ReData.DemoApp.Repositories.Datasets;

public interface IDatasetRepository
{
    Task<List<DataSetListItem>> GetAllAsync(CancellationToken ct);

    Task<DataSetEntity?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<DataSetEntity?> GetByNameAsync(string name, CancellationToken ct);

    Task<DataSetEntity> CreateAsync(CreateDatasetData data, CancellationToken ct);

    Task<bool> UpdateAsync(UpdateDatasetData data, CancellationToken ct);

    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
