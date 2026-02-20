namespace ReData.Query.Core.Types;

/// <summary>
/// Аргумент функции.
/// Содержит информацию о параметре, который передается в функцию.
/// </summary>
public record FunctionArgument
{
    
    /// <summary>
    /// Название аргумента.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Тип аргумента.
    /// </summary>
    public required FunctionArgumentType Type { get; init; }
    
    /// <summary>
    /// Определяет пропагирует ли аргумент null если сам будет null
    /// </summary>
    public required bool PropagateNull { get; init; }

    /// <summary>
    /// Требует константный аргумент (литерал или константа).
    /// </summary>
    public bool IsConstRequired { get; init; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Name}: {Type}";
    }
}

/// <summary>
/// Тип аргумента функции.
/// </summary>
public record FunctionArgumentType
{
    /// <summary>
    /// Базовый тип данных.
    /// </summary>
    public required DataType DataType { get; init; }

    /// <summary>
    /// Может ли аргумент принимать NULL.
    /// </summary>
    public required bool CanBeNull { get; init; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{DataType.Display()}{(CanBeNull ? "" : "!")}";
    }
}
