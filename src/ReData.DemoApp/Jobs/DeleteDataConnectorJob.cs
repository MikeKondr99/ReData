using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using ReData.DemoApp.Database;
using TickerQ.Utilities.Base;

namespace ReData.DemoApp.Jobs;

public sealed class DeleteDataConnectorJob
{
    private ApplicationDatabaseContext Db { get; init; }
    private IOutputCacheStore OutputCache { get; init; }

    public DeleteDataConnectorJob(ApplicationDatabaseContext db, IOutputCacheStore outputCache)
    {
        Db = db;
        OutputCache = outputCache;
    }

    [TickerFunction("Delete data connector")]
    public async Task DeleteDataConnector(
        TickerFunctionContext<DeleteDataConnectorRequest> context,
        CancellationToken ct)
    {
        var name = context.Request.DataConnectorName;
        var dataConnector = await Db.DataConnectors
            .FirstOrDefaultAsync(dc => dc.Name == name && dc.TableName.StartsWith("table"), ct);
        if (dataConnector is null)
        {
            return;
        }

        Db.DataConnectors.Remove(dataConnector);
        await Db.SaveChangesAsync(ct);
        await OutputCache.EvictByTagAsync("data-connectors", ct);
    }
}