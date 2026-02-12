using System.Data.Common;
using ClickHouse.Client.Numerics;
using ReData.Query.Core.Types;

namespace ReData.Query.Runners;

/// <summary>
/// Приводит значения провайдеров БД к стабильному CLR-представлению.
/// Поддерживаемые входные типы:
/// <list type="bullet">
/// <item><description><see langword="null"/> и <see cref="DBNull"/>.</description></item>
/// <item><description>Целые: <see cref="sbyte"/>, <see cref="byte"/>, <see cref="short"/>, <see cref="ushort"/>, <see cref="int"/>, <see cref="uint"/>, <see cref="long"/>, <see cref="ulong"/>.</description></item>
/// <item><description>Числа с плавающей точкой: <see cref="float"/>, <see cref="double"/>, <see cref="decimal"/>, <see cref="ClickHouseDecimal"/>.</description></item>
/// <item><description><see cref="bool"/>.</description></item>
/// <item><description><see cref="DateTime"/>, <see cref="DateTimeOffset"/>.</description></item>
/// <item><description><see cref="string"/> и <see cref="char"/>.</description></item>
/// </list>
/// Выходные типы:
/// <list type="bullet">
/// <item><description><see langword="null"/>.</description></item>
/// <item><description><see cref="long"/> для целых.</description></item>
/// <item><description><see cref="double"/> для чисел с плавающей точкой.</description></item>
/// <item><description><see cref="bool"/>.</description></item>
/// <item><description><see cref="DateTime"/> (UTC для входного <see cref="DateTimeOffset"/>).</description></item>
/// <item><description><see cref="string"/>.</description></item>
/// </list>
/// Для неподдерживаемых типов или несовместимости с ожидаемым <see cref="FieldType"/> выбрасывается исключение.
/// </summary>
#pragma warning disable CA1010
public sealed class ClrMappedDbDataReader : DbDataReaderDecorator
{
    private readonly FieldType[] fieldTypes;

    public ClrMappedDbDataReader(DbDataReader inner, IEnumerable<Field> fields)
        : base(inner)
    {
        fieldTypes = fields.Select(f => f.Type).ToArray();
    }

    public override object GetValue(int ordinal)
    {
        if ((uint)ordinal >= (uint)fieldTypes.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(ordinal), ordinal, "Ожидаемый тип поля не найден.");
        }

        var raw = Inner.GetValue(ordinal);
        var expected = fieldTypes[ordinal];
        var normalized = Normalize(raw, expected);

        EnsureCompatibleWithExpectedType(normalized, expected);
        return normalized ?? null!;
    }

    public override int GetValues(object[] values)
    {
        var count = Math.Min(values.Length, FieldCount);
        for (var i = 0; i < count; i++)
        {
            values[i] = GetValue(i);
        }

        return count;
    }

    public override bool IsDBNull(int ordinal) => GetValue(ordinal) is null;

    private static object? Normalize(object? value, FieldType expectedType)
    {
        if (value is null or DBNull)
        {
            return null;
        }

        return value switch
        {
            sbyte v => (long)v,
            byte v => (long)v,
            short v => v,
            ushort v => v,
            int v => v,
            uint v => v,
            long v => v,
            ulong v when v <= long.MaxValue => (long)v,
            ulong v => throw new OverflowException($"Значение ulong '{v}' не помещается в Int64."),
            float v => v,
            double v => v,
            decimal v => (double)v,
            ClickHouseDecimal v => (double)v,
            bool v => v,
            DateTime v => v,
            DateTimeOffset dto => DateTime.SpecifyKind(dto.UtcDateTime, DateTimeKind.Utc),
            string v => v,
            char v => v.ToString(),
            _ => throw new NotSupportedException(
                $"Тип '{value.GetType().FullName}' не поддерживается в {nameof(ClrMappedDbDataReader)} для ожидаемого типа '{expectedType.Type}'."),
        };
    }

    private static void EnsureCompatibleWithExpectedType(object? value, FieldType expectedType)
    {
        if (value is null)
        {
            return;
        }

        var ok = expectedType.Type switch
        {
            DataType.Text => value is string,
            DataType.Integer => value is long,
            DataType.Number => value is double or long,
            DataType.Bool => value is bool or long,
            DataType.DateTime => value is DateTime,
            DataType.Null => false,
            DataType.Unknown => value is string or long or double or bool or DateTime,
            _ => false,
        };

        if (!ok)
        {
            throw new InvalidCastException(
                $"Значение типа '{value.GetType().FullName}' несовместимо с ожидаемым типом поля '{expectedType.Type}'.");
        }
    }
}
#pragma warning restore CA1010
