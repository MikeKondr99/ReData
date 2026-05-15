using Microsoft.Extensions.Logging;
using TickerQ.Utilities.Base;
using TickerQ.Utilities.Entities;
using TickerQ.Utilities.Interfaces;
using TickerQ.Utilities.Interfaces.Managers;

namespace ReData.Jobs;

/// <summary>
/// Minimal cross-project TickerQ job used as the first extracted job host contract.
/// Existing DemoApp jobs can be moved here incrementally.
/// </summary>
public sealed class TestJob(
    ILogger<TestJob> logger,
    ITimeTickerManager<TimeTickerEntity> timeTickerManager) : ITickerFunction<TestJobRequest>
{
    public async Task ExecuteAsync(
        TickerFunctionContext<TestJobRequest> context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var request = Normalize(context.Request);
        if (request.JobCount > 1)
        {
            await FanOutJobsAsync(request, context, cancellationToken);
            return;
        }

        var delayMs = Random.Shared.Next(request.MinDelayMs, request.MaxDelayMs + 1);
        logger.LogInformation(
            "Executing test TickerQ job {JobId} with delay {DelayMs}ms. Scheduled for {ScheduledFor}.",
            context.Id,
            delayMs,
            context.ScheduledFor);

        await Task.Delay(delayMs, cancellationToken);

        logger.LogInformation(
            "Completed test TickerQ job {JobId} after {DelayMs}ms.",
            context.Id,
            delayMs);
    }

    private async Task FanOutJobsAsync(
        TestJobRequest request,
        TickerFunctionContext<TestJobRequest> context,
        CancellationToken cancellationToken)
    {
        var executionTime = DateTime.UtcNow;

        for (var index = 0; index < request.JobCount; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var childDelayMs = Random.Shared.Next(request.MinDelayMs, request.MaxDelayMs + 1);
            var result = await timeTickerManager.AddAsync<TestJob, TestJobRequest>(
                executionTime,
                new TestJobRequest
                {
                    JobCount = 1,
                    MinDelayMs = childDelayMs,
                    MaxDelayMs = childDelayMs
                },
                cancellationToken);

            if (!result.IsSucceeded)
            {
                throw result.Exception ?? new InvalidOperationException("TickerQ failed to schedule replica test jobs.");
            }
        }

        logger.LogInformation(
            "Expanded test TickerQ job {JobId} into {JobCount} child jobs with delay range {MinDelayMs}-{MaxDelayMs}ms.",
            context.Id,
            request.JobCount,
            request.MinDelayMs,
            request.MaxDelayMs);
    }

    private static TestJobRequest Normalize(TestJobRequest? request)
    {
        if (request is null)
        {
            return new TestJobRequest
            {
                JobCount = 1,
                MinDelayMs = 1000,
                MaxDelayMs = 1000
            };
        }

        var jobCount = Math.Max(1, request.JobCount);
        var minDelayMs = Math.Max(0, request.MinDelayMs);
        var maxDelayMs = Math.Max(minDelayMs, request.MaxDelayMs);

        return new TestJobRequest
        {
            JobCount = jobCount,
            MinDelayMs = minDelayMs,
            MaxDelayMs = maxDelayMs
        };
    }
}
