using System.Data.Common;
using ReData.Query.Core.Types;
using ReData.Query.Core.Value;
using ReData.Query.Runners.Value;

namespace ReData.Query.Runners;

/// <summary>
/// Legacy-адаптер: преобразует нормализованные CLR-значения в IValue.
/// </summary>
#pragma warning disable CA1010
public sealed class LegacyIValueDbDataReader : DbDataReaderDecorator
{
    private readonly FieldType[] fieldTypes;

    public LegacyIValueDbDataReader(DbDataReader inner, IEnumerable<Field> fields)
        : base(inner)
    {
        fieldTypes = fields.Select(f => f.Type).ToArray();
    }

    public override object GetValue(int ordinal)
    {
        var raw = Inner.GetValue(ordinal);
        var expectedType = fieldTypes[ordinal];
        return MapField(raw, expectedType);
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

    public override bool IsDBNull(int ordinal) => GetValue(ordinal) is NullValue;

    private static IValue MapField(object? obj, FieldType expectedType)
    {
        if (obj is IValue iv)
        {
            return iv;
        }

        var value = MapFromObject(obj);

        return (value, expectedType.Type) switch
        {
            (IntegerValue, DataType.Integer) => value,
            (NumberValue, DataType.Number) => value,
            (TextValue, DataType.Text) => value,
            (DateTimeValue, DataType.DateTime) => value,
            (BoolValue, DataType.Bool) => value,
            (NullValue, DataType.Null) => value,
            (UnknownValue, DataType.Unknown) => value,
            (NullValue, _) when expectedType.CanBeNull => value,
            (NullValue, _) when !expectedType.CanBeNull => throw new Exception($"Ожидалось что значение не 'NULL' типа {expectedType.Type}, А получилось null"),
            (IntegerValue(var v), DataType.Bool) => new BoolValue(v != 0),
            (NumberValue(var v), DataType.Integer) => new IntegerValue((long)v),
            (IntegerValue(var v), DataType.Number) => new NumberValue(v),
            _ => throw new Exception($"Неудалось замапить тип {value} к {expectedType.Type}")
        };
    }

    private static IValue MapFromObject(object? value) => value switch
    {
        sbyte v => new IntegerValue(v),
        short v => new IntegerValue(v),
        int v => new IntegerValue(v),
        long v => new IntegerValue(v),

        byte v => new IntegerValue(v),
        ushort v => new IntegerValue(v),
        uint v => new IntegerValue(v),
        ulong v when v <= long.MaxValue => new IntegerValue((long)v),
        ulong v => new NumberValue(v),

        bool b => new BoolValue(b),

        double v => new NumberValue(v),
        float v => new NumberValue(v),
        decimal v => new NumberValue((double)v),

        DateTime v => new DateTimeValue(v),
        DateTimeOffset v => new DateTimeValue(DateTime.SpecifyKind(v.UtcDateTime, DateTimeKind.Utc)),

        string v => new TextValue(v),

        DBNull => default(NullValue),
        null => default(NullValue),

        _ => new UnknownValue(value.GetType().Name),
    };
}
#pragma warning restore CA1010
