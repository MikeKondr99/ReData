using ReData.Query.Core.Template;

namespace ReData.Query.Core.Types;

/// <summary>
/// Структура описывает некое поле из запроса которое можно использовать работая с этим запросом
/// </summary>
public record struct Field
{
    /// <summary>
    /// Алиас для конечного пользователя
    /// </summary>
    public required string Alias { get; init; }
    
    /// <summary>
    /// Как обратиться к этому полю на уровне конечного запроса (SQL)
    /// </summary>
    public required ITemplate Template { get; init; }
    
    /// <summary>
    /// Тип поля
    /// </summary>
    public required FieldType Type { get; init; }
}