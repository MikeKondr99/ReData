namespace ReData.DemoApp.Endpoints.Transform;

/// <summary>
/// POC-ответ для array-версии трансформаций.
/// </summary>
public sealed record TransformArrayResponse
{
    public required IReadOnlyList<TransformFieldViewModel> Fields { get; init; }

    public required long Total { get; init; }

    public required object[] Data { get; init; }
}
