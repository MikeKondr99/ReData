using ClickHouse.Client.Numerics;
using ReData.Query.Core.Types;
using ReData.Query.Runners.Value;

namespace ReData.Query.Runners;

public class DatabaseValuesMapper
{
    public IValue MapField(object? obj, FieldType expectedType)
    {
        var value = MapFromObject(obj);

        return (value, expectedType.Type) switch
        {
            // One to one
            (IntegerValue, DataType.Integer) => value,
            (NumberValue, DataType.Number) => value,
            (TextValue, DataType.Text) => value,
            (DateTimeValue, DataType.DateTime) => value,
            (BoolValue, DataType.Bool) => value,
            (NullValue, DataType.Null) => value,
            (UnknownValue, DataType.Unknown) => value,
            // Null
            (NullValue, _) when expectedType.CanBeNull => value,
            (NullValue, _) when !expectedType.CanBeNull => throw new Exception($"Ожидалось что значение не 'NULL' типа {expectedType.Type}, А получилось null"),
            // Casts
            (IntegerValue(var v), DataType.Bool) => new BoolValue(v != 0),
            (NumberValue(var v), DataType.Integer) => new IntegerValue((long)v),
            (IntegerValue(var v), DataType.Number) => new NumberValue(v),
            _ => throw new Exception($"Неудалось замапить тип {value} к {expectedType.Type}")
        };

    }

    private IValue MapFromObject(object? value) => value switch
    {
        // int
        sbyte v => new IntegerValue(v),
        short v => new IntegerValue(v),
        int v => new IntegerValue(v),
        long v => new IntegerValue(v),
        // uint
        byte v => new IntegerValue(v),
        ushort v => new IntegerValue(v),
        uint v => new IntegerValue(v),
        ulong v => new IntegerValue((long)v),
        // bool
        bool b => new BoolValue(b),
        // floats
        double v => new NumberValue(v),
        float v => new NumberValue(v),
        decimal v => new NumberValue((double)v),
        // Datetime
        DateTime v => new DateTimeValue(v),
        DateTimeOffset v => new DateTimeValue(DateTime.SpecifyKind(v.DateTime, DateTimeKind.Unspecified)),

        ClickHouseDecimal v => new NumberValue((double)v),
        // text
        string v => new TextValue(v),

        // null
        DBNull => new NullValue(),
        null => new NullValue(),
        // unknown
        _ => new UnknownValue(value.GetType().Name),
    };
}