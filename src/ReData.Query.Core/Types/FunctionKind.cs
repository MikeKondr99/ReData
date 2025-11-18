namespace ReData.Query.Core.Types;

/// <summary>
/// Категория функции, определяющая способ её вызова и синтаксис использования.
/// </summary>
public enum FunctionKind
{
    /// <summary>
    /// Стандартная функция. Вызывается как Name(arg1, arg2, ...)
    /// </summary>
    Default = 0,

    /// <summary>
    /// Метод. Может вызываться как в стиле функции Name(arg1, arg2), 
    /// так и в стиле метода arg1.Name(arg2).
    /// Пример: Add(a, b) или a.Add(b)
    /// </summary>
    Method = 1,

    /// <summary>
    /// Бинарная операция. Вызывается в инфиксной нотации: arg1 Name arg2.
    /// Пример: a + b, x == y
    /// </summary>
    Binary = 2,

    /// <summary>
    /// Унарная операция. Вызывается в префиксной или постфиксной нотации: Name arg или arg Name.
    /// Пример: -x
    /// </summary>
    Unary = 3,
}