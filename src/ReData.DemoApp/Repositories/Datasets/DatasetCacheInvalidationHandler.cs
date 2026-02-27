using FastEndpoints;
using Z.EntityFramework.Plus;

namespace ReData.DemoApp.Repositories.Datasets;

public sealed class DatasetCacheInvalidationHandler : IEventHandler<DatasetChangedEvent>
{
    public Task HandleAsync(DatasetChangedEvent eventModel, CancellationToken ct)
    {
        QueryCacheManager.ExpireTag($"dataset:{eventModel.DatasetId}", "dataset:list");
        return Task.CompletedTask;
    }
}
