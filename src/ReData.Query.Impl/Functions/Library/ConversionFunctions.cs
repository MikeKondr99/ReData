using System.Runtime.InteropServices.JavaScript;
using ReData.Core;
using ReData.Query.Visitors;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;
using static DataType;

public class ConversionFunctions : FunctionsDescriptor
{
    private FunctionBuilder Convertion(DataType input, DataType ret)
    {
        var name = ret switch
        {
            Unknown => "Unknown",
            Null => "Null",
            Number => "Num",
            Integer => "Int",
            DataType.Text => "Text",
            Boolean => "Bool",
            var any => throw new UnknownEnumValueException<DataType>(any),
        };
        return Method(name)
            .Arg("input", input)
            .Returns(ret);
    }
    protected override void Functions()
    {
        // Noop Conversions
        foreach (var T in new DataType[] { Number, Integer, Text, Boolean})
        {
            Convertion(T,T)
                .Templates(new()
                {
                    [All] = $"{0}",
                });
        }
        Convertion(Text,Integer)
            .Templates(new()
            {
                [SqlServer] = $"CAST({0} AS INTEGER)",
                [MySql] = $"CAST({0} AS SIGNED)",
                [PostgreSql | Oracle] = $"CAST({0} AS INTEGER)",
                [ClickHouse] = $"CAST({0} AS Int64)",
            });
        
        Convertion(Boolean,Integer)
            .Templates(new()
            {
                [All] = $"CASE WHEN {0} THEN 1 ELSE 0 END",
            });
        
        Convertion(Number,Integer)
            .Templates(new()
            {
                [SqlServer] = $"CAST({0} AS INTEGER)",
                [PostgreSql | Oracle] = $"CAST(FLOOR({0}) AS INTEGER)",
                [MySql] = $"CAST(FLOOR({0}) AS SIGNED)",
                [ClickHouse] = $"CAST({0} AS Int64)"
            });
        
        Convertion(Null,Integer)
            .Templates(new()
            {
                [All] = $"({0} + 0)",
            });
        
        Convertion(Text,Number)
            .Templates(new()
            {
                [All & ~ClickHouse &~Oracle] = $"CAST({0} AS DECIMAL(20,10))",
                [Oracle] = $"TO_NUMBER({0})",
                [ClickHouse] = $"toDecimal64({0}, 10)"
            });
        
        Convertion(Boolean,Number)
            .Templates(new()
            {
                [All] = $"CASE WHEN {0} THEN 1.0 ELSE 0.0 END"
            });
        
        Convertion(Integer, Number)
            .Templates(new()
            {
                [All & ~ClickHouse &~ Oracle] = $"CAST({0} AS DECIMAL(30,15))",
                [Oracle] = $"CAST({0} AS NUMERIC)",
                [ClickHouse] = $"toDecimal64({0}, 10)"
            });
        
        Convertion(Null, Number)
            .Templates(new()
            {
                [All] = $"({0} + 0.0)",
            });
        
        Convertion(Text, Boolean)
            .Templates(new()
            {
                [SqlServer] = $"LEN({0}) > 0",
                [MySql | PostgreSql | ClickHouse | Oracle] = $"LENGTH({0}) > 0"
            });
        
        Convertion(Number, Boolean)
            .Templates(new()
            {
                [All] = $"({0} > 0.0)"
            });
        
        Convertion(Integer, Boolean)
            .Templates(new()
            {
                [All] = $"({0} > 0)"
            });
        
        Convertion(Null, Boolean)
            .Templates(new()
            {
                [All] = $"({0} = 0)",
            });
        
        Convertion(Boolean, Text)
            .Templates(new()
            {
                [All] = $"CASE WHEN {0} THEN 'true' ELSE 'false' END"
            });
        
        Convertion(Number, Text)
            .Templates(new()
            {
                [All & ~ (MySql | Oracle)] = $"CAST({0} AS VARCHAR)",
                [MySql] = $"CAST({0} AS CHAR)",
                [Oracle] = $"REPLACE(TO_CHAR({0}),',','.')"
            });
        
        Convertion(Integer, Text)
            .Templates(new()
            {
                [All & ~MySql & ~Oracle] = $"CAST({0} AS VARCHAR)",
                [MySql] = $"CAST({0} AS CHAR)",
                [Oracle] = $"TO_CHAR({0})"
            });
        
        Convertion(Null, Text)
            .Templates(new()
            {
                [All] = $"LOWER({0})",
            });
    }
}