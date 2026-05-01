using ReData.DemoApp.Database;
using TickerQ.Utilities.Base;
using TickerQ.Utilities.Enums;

namespace ReData.DemoApp.Jobs;

// ReSharper disable once UnusedMember.Global
public class HealthCheckJobs
{
    private ApplicationDatabaseContext Db { get; init; }

    public HealthCheckJobs(ApplicationDatabaseContext db)
    {
        Db = db;
    }

    [TickerFunction("Database Healthcheck", TickerTaskPriority.Low)]
    public async Task DatabaseHealthCheck(
        TickerFunctionContext context,
        CancellationToken ct)
    {
        if (await Db.Database.CanConnectAsync(ct))
        {
            return;
        }

        throw new Exception("Job failed");
    }
}
