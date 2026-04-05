using ReData.Query.Common;

namespace ReData.DemoApp.Endpoints.Transform;

public sealed record TransformErrorResponse
{
    /// <summary>
    /// Номер включенной трансформации в которой была обнаружена ошибка
    /// </summary>
    public required int Index { get; init; }
    
    /// <summary>
    /// Сообщение об найденной ошибке
    /// </summary>
    public required string Message { get; init; }
    
    /// <summary>
    /// Ошибки найденные при работе с выражениями
    /// </summary>
    public required IEnumerable<IReadOnlyList<ExprError>>? Errors { get; init; }
}