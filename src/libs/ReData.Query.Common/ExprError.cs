namespace ReData.Query.Common;

public record ExprError
{
    /// <summary>
    /// Диапазон где была найдена ошибка
    /// </summary>
    public required ExprSpan Span { get; init; }
    
    /// <summary>
    /// Сообщение об ошибке
    /// </summary>
    public required string Message { get; init; }
}