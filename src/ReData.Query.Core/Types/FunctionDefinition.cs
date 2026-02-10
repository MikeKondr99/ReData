using Dunet;
using ReData.Query.Core.Template;

namespace ReData.Query.Core.Types;

/// <summary>
/// Внутреннее определение функции.
/// Содержит полную метаинформацию о функции, включая сигнатуру, констатность и как пропагируется null.
/// </summary>
public sealed record FunctionDefinition
{
    /// <summary>
    /// Название функции. Используется для идентификации и вызова.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Документация по функции. Может быть null если не указана.
    /// </summary>
    public required string? Doc { get; init; }

    /// <summary>
    /// Список аргументов функции в порядке их объявления.
    /// </summary>
    public required IReadOnlyList<FunctionArgument> Arguments { get; init; }

    /// <summary>
    /// Тип возвращаемого значения функции.
    /// </summary>
    public required FunctionReturnType ReturnType { get; init; }

    /// <summary>
    /// Категория функции (бинарная, унарная и т.д.). Влияет на отображение и парсинг.
    /// </summary>
    public required FunctionKind Kind { get; init; }

    /// <summary>
    /// Шаблон выполнения функции. Во что компилируется функция.
    /// </summary>
    public required IFunctionTemplate Template { get; init; }

    /// <summary>
    /// Метаданные неявного приведения типов. Если не null функция является функцией неявного приведения.
    /// </summary>
    public required ImplicitCastMetadata? ImplicitCast { get; init; }

    /// <summary>
    /// Кастомная функция распространения null значений. 
    /// Если null, используется стандартная логика распространения на основе PropagateNull аргументов.
    /// </summary>
    public required Func<IEnumerable<bool>, bool>? CustomNullPropagation { get; init; }

    /// <summary>
    /// Правила распространения константности.
    /// </summary>
    public required ConstPropagation ConstPropagation { get; init; }


    private string? cacheToString;

    /// <inheritdoc/>
    public override string ToString()
    {
        if (cacheToString is null)
        {
            if (Kind is FunctionKind.Binary)
            {
                cacheToString = $"({Arguments[0].Type} {Name} {Arguments[1].Type}) -> {ReturnType}";
            }
            else
            {
                cacheToString = $"{Name}({string.Join(", ", Arguments.Select(a => $"{a}"))}) -> {ReturnType}";
            }
        }

        return cacheToString;
    }
}

/// <summary>
/// Стратегия распространения констант.
/// Определяет, считается ли функция константной в зависимости от аргументов
/// </summary>
public enum ConstPropagation
{
    /// <summary>
    /// Является константной если все аргументы константны
    /// </summary>
    Default = 1,
    
    /// <summary>
    /// Всегда является константной. Например, функция Type
    /// </summary>
    AlwaysTrue = 2,
    
    /// <summary>
    /// Всегда не является константной. Например, функция Now
    /// </summary>
    AlwaysFalse = 3,
}
