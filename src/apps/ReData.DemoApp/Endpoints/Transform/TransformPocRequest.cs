namespace ReData.DemoApp.Endpoints.Transform;

/// <summary>
/// Запрос для POC endpoint-ов transform.
/// </summary>
public sealed record TransformPocRequest
{
    /// <summary>
    /// Идентификатор коннектора. По умолчанию <see cref="Guid.Empty"/>.
    /// </summary>
    public Guid ConnectorId { get; init; } = Guid.Empty;
}
