using System.Text.Json.Serialization;

namespace ReData.Query.Core.Types;

/// <summary>
/// Базовые типы данных в системе типов.
/// Определяет примитивные типы, с которыми работают функции.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<DataType>))]
public enum DataType
{
    /// <summary>
    /// Неизвестный тип. Используется для ошибок типизации или когда тип не может быть определен.
    /// </summary>
    [JsonStringEnumMemberName("unk")]
    Unknown = 0,

    /// <summary>
    /// Тип NULL. Используется для представления отсутствующего значения.
    /// </summary>
    [JsonStringEnumMemberName("null")]
    Null = 1,

    /// <summary>
    /// Число с плавающей точкой.
    /// </summary>
    [JsonStringEnumMemberName("num")]
    Number = 2,

    /// <summary>
    /// Целое число. Аналог int/long в языках программирования.
    /// </summary>
    [JsonStringEnumMemberName("int")]
    Integer = 3,

    /// <summary>
    /// Текстовый тип. Аналог string в языках программирования.
    /// </summary>
    [JsonStringEnumMemberName("text")]
    Text = 4,

    /// <summary>
    /// Логический тип. Значения true/false.
    /// </summary>
    [JsonStringEnumMemberName("bool")]
    Bool = 5,

    /// <summary>
    /// Дата и время. Используется для временных меток и операций с датами.
    /// </summary>
    [JsonStringEnumMemberName("date")]
    DateTime = 6,
}