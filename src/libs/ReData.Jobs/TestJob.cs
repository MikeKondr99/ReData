using Microsoft.Extensions.Logging;
using TickerQ.Utilities.Base;
using TickerQ.Utilities.Interfaces;

namespace ReData.Jobs;

/// <summary>
/// Minimal cross-project TickerQ job used as the first extracted job host contract.
/// Existing DemoApp jobs can be moved here incrementally.
/// </summary>
public sealed class TestJob(ILogger<TestJob> logger) : ITickerFunction
{
    public Task ExecuteAsync(TickerFunctionContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        logger.LogInformation(
            "Executing test TickerQ job {JobId} scheduled for {ScheduledFor}.",
            context.Id,
            context.ScheduledFor);

        return Task.CompletedTask;
    }
}
