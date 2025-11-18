using ReData.Query.Core.Types;

namespace ReData.DemoApplication.Endpoints.Functions;

/// <summary>
/// Описание функции, доступной для вызова в системе.
/// Предназначено для отображения пользователю списка всех доступных функций с их параметрами и документацией.
/// </summary>
public sealed record FunctionResponse
{
    /// <summary>
    /// Название функции для вызова.
    /// Используется в синтаксисе вызова: Name(аргумент1, аргумент2)
    /// </summary>
    /// <example>Sign</example>
    public required string Name { get; init; }

    /// <summary>
    /// Документация по функции - описание что делает функция и как её использовать.
    /// Может быть null если документация отсутствует.
    /// </summary>
    /// <example>Получает знак числа</example>
    public required string? Doc { get; init; }

    /// <summary>
    /// Список аргументов функции в порядке их передачи.
    /// Определяет сигнатуру вызова: Name(аргумент1, аргумент2)
    /// </summary>
    public required IReadOnlyList<FunctionArgument> Arguments { get; init; }

    /// <summary>
    /// Тип возвращаемого значения функции.
    /// Описывает что вернет функция после выполнения.
    /// </summary>
    public required FunctionReturnType ReturnType { get; init; }

    /// <summary>
    /// Категория или тип функции.
    /// Определяет поведение и контекст использования функции.
    /// </summary>
    /// <example>Method</example>
    public required FunctionKind Kind { get; init; }

    /// <summary>
    /// Текстовое представление функции для отображения пользователю.
    /// Обычно содержит сигнатуру функции в читаемом формате.
    /// </summary>
    public required string DisplayText { get; init; }
}