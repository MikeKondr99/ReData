namespace ReData.Query.Core.Types;

/// <summary>
/// Тип возвращаемого значения функции.
/// Определяет тип данных, возможность быть NULL и является ли результат агрегированным.
/// </summary>
public sealed record FunctionReturnType
{
    /// <summary>
    /// Базовый тип данных возвращаемого значения.
    /// </summary>
    public required DataType DataType { get; init; }

    /// <summary>
    /// Может ли функция возвращать NULL значение.
    /// </summary>
    public required bool CanBeNull { get; init; }
    
    /// <summary>
    /// Является ли функция агрегирующей (вычисляющей значение по набору данных).
    /// true - функция агрегирует данные (например, SUM, COUNT)
    /// false - функция работает с отдельными значениями
    /// </summary>
    public bool Aggregated { get; init; }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Aggregated)
        {
            return $"agg<{DataType.Display()}{(CanBeNull ? "" : "!")}>";
        }
        return $"{DataType.Display()}{(CanBeNull ? "" : "!")}";
    }
}