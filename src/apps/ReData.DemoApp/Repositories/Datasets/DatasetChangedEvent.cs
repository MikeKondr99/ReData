using FastEndpoints;
using ReData.DemoApp.Database.Entities;
using StrictId;

namespace ReData.DemoApp.Repositories.Datasets;

public enum DatasetMutationType
{
    Created,
    Updated,
    Deleted,
}

public sealed record DatasetChangedEvent : IEvent
{
    public required Id<DatasetEntity> DatasetId { get; init; }

    public required DatasetMutationType MutationType { get; init; }

    public required DateTimeOffset OccurredAt { get; init; }

    public required IReadOnlyList<string> AffectedNames { get; init; }
}
