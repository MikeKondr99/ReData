namespace ReData.DemoApp.Endpoints.Transform;

/// <summary>
/// Ответ после применения трансформаций
/// </summary>
public sealed record TransformResponse
{
    /// <summary>
    /// Полученные поля после применения трансформаций
    /// </summary>
    public required IReadOnlyList<TransformFieldViewModel> Fields { get; init; }

    /// <summary>
    /// Общее количество данных подходящих под составленный запрос
    /// </summary>
    public required long? Total { get; init; }

    /// <summary>
    /// Одна страница данных соответствующих запросу
    /// </summary>
    public required IAsyncEnumerable<object> Data { get; init; }
}
