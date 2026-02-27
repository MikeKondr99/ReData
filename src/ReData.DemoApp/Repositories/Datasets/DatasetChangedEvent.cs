using FastEndpoints;

namespace ReData.DemoApp.Repositories.Datasets;

public enum DatasetMutationType
{
    Created,
    Updated,
    Deleted,
}

public sealed record DatasetChangedEvent : IEvent
{
    public required Guid DatasetId { get; init; }

    public required DatasetMutationType MutationType { get; init; }

    public required DateTimeOffset OccurredAt { get; init; }
}
