namespace ReData.DemoApp.Endpoints.Transform;

/// <summary>
/// POC-ответ для stream-версии трансформаций.
/// </summary>
public sealed record TransformStreamResponse
{
    public required IReadOnlyList<TransformFieldViewModel> Fields { get; init; }

    public required long Total { get; init; }

    public required IAsyncEnumerable<object> Data { get; init; }
}
