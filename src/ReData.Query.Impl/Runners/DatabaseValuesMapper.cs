using ClickHouse.Client.Numerics;

namespace ReData.Query.Impl.Runners;

public class DatabaseValuesMapper
{
    public IValue MapField(object? value)
    {
        return value switch
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
}