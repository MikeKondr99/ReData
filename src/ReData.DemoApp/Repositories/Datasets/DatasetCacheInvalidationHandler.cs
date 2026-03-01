using FastEndpoints;
using Z.EntityFramework.Plus;

namespace ReData.DemoApp.Repositories.Datasets;

public sealed class DatasetCacheInvalidationHandler : IEventHandler<DatasetChangedEvent>
{
    public Task HandleAsync(DatasetChangedEvent eventModel, CancellationToken ct)
    {
        var tags = new List<string>(2 + eventModel.AffectedNames.Count)
        {
            DatasetCacheTags.ById(eventModel.DatasetId),
            DatasetCacheTags.List,
        };

        for (int i = 0; i < eventModel.AffectedNames.Count; i++)
        {
            tags.Add(DatasetCacheTags.ByName(eventModel.AffectedNames[i]));
        }

        QueryCacheManager.ExpireTag(tags.ToArray());
        return Task.CompletedTask;
    }
}
