using System.Runtime.InteropServices.JavaScript;
using ReData.Core;
using ReData.Query.Visitors;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;
using static DataType;

public class ConversionFunctions : FunctionsDescriptor
{
    private FunctionBuilder Conversion(DataType input, DataType ret)
    {
        var name = ret switch
        {
            Unknown => "Unknown",
            Null => "Null",
            Number => "Num",
            Integer => "Int",
            Text => "Text",
            DateTime => "Date",
            Bool => "Bool",
            var any => throw new UnknownEnumValueException<DataType>(any),
        };
        return Method(name)
            .Arg("input", input)
            .Returns(ret);
    }
    protected override void Functions()
    {
        // Noop Conversions
        foreach (var T in new DataType[] { Number, Integer, Text, Bool, DateTime})
        {
            Conversion(T,T)
                .Templates(new()
                {
                    [All] = $"{0}",
                });
        }
        
        Conversion(Text,Integer)
            .Templates(new()
            {
                [SqlServer] = $"CAST({0} AS INTEGER)",
                [MySql] = $"CAST({0} AS SIGNED)",
                [PostgreSql | Oracle] = $"CAST({0} AS INTEGER)",
                [ClickHouse] = $"CAST({0} AS Int64)",
            });
        
        Conversion(Bool,Integer)
            .Templates(new()
            {
                [All] = $"CASE WHEN {0} THEN 1 ELSE 0 END",
            });
        
        Conversion(Number,Integer)
            .Templates(new()
            {
                [SqlServer] = $"CAST({0} AS INTEGER)",
                [PostgreSql | Oracle] = $"CAST(FLOOR({0}) AS INTEGER)",
                [MySql] = $"CAST(FLOOR({0}) AS SIGNED)",
                [ClickHouse] = $"CAST({0} AS Int64)"
            });
        
        Conversion(Null,Integer)
            .Templates(new()
            {
                [All] = $"({0} + 0)",
            });
        
        Conversion(Text,Number)
            .Templates(new()
            {
                [All & ~ClickHouse &~Oracle] = $"CAST({0} AS DECIMAL(20,10))",
                [Oracle] = $"TO_NUMBER({0})",
                [ClickHouse] = $"toDecimal64({0}, 10)"
            });
        
        Conversion(Bool,Number)
            .Templates(new()
            {
                [All] = $"CASE WHEN {0} THEN 1.0 ELSE 0.0 END"
            });
        
        Conversion(Integer, Number)
            .Templates(new()
            {
                [All & ~ClickHouse &~ Oracle] = $"CAST({0} AS DECIMAL(30,15))",
                [Oracle] = $"CAST({0} AS NUMERIC)",
                [ClickHouse] = $"toDecimal64({0}, 10)"
            });
        
        Conversion(Null, Number)
            .Templates(new()
            {
                [All] = $"({0} + 0.0)",
            });
        
        Conversion(Text, Bool)
            .Templates(new()
            {
                [SqlServer] = $"LEN({0}) > 0",
                [MySql | PostgreSql | ClickHouse | Oracle] = $"LENGTH({0}) > 0"
            });
        
        Conversion(Number, Bool)
            .Templates(new()
            {
                [All] = $"({0} > 0.0)"
            });
        
        Conversion(Integer, Bool)
            .Templates(new()
            {
                [All] = $"({0} > 0)"
            });
        
        Conversion(Null, Bool)
            .Templates(new()
            {
                [All] = $"({0} = 0)",
            });
        
        Conversion(Bool, Text)
            .Templates(new()
            {
                [All] = $"CASE WHEN {0} THEN 'true' ELSE 'false' END"
            });
        
        Conversion(Number, Text)
            .Templates(new()
            {
                [All & ~ (MySql | Oracle)] = $"CAST({0} AS VARCHAR)",
                [MySql] = $"CAST({0} AS CHAR)",
                [Oracle] = $"REPLACE(TO_CHAR({0}),',','.')"
            });
        
        Conversion(Integer, Text)
            .Templates(new()
            {
                [All & ~MySql & ~Oracle] = $"CAST({0} AS VARCHAR)",
                [MySql] = $"CAST({0} AS CHAR)",
                [Oracle] = $"TO_CHAR({0})"
            });
        
        Conversion(DateTime, Text)
            .Templates(new()
            {
                [SqlServer] = $"CONVERT(NVARCHAR(20), {0}, 120)", // ODBC canonical format
                [ClickHouse] = $"formatDateTime({0}, '%Y-%m-%d %H:%i:%S')",
                [MySql] = $"DATE_FORMAT({0}, '%Y-%m-%d %H:%i:%S')",
                [PostgreSql] = $"TO_CHAR({0}, 'YYYY-MM-DD HH24:MI:SS')",
                [Oracle] = $"TO_CHAR({0}, 'YYYY-MM-DD HH24:MI:SS')",
            });
        
        Conversion(Null, Text)
            .Templates(new()
            {
                [All] = $"LOWER({0})",
            });
        
        
        Conversion(Text, DateTime)
            .Templates(new()
            {
                [ClickHouse] = $"parseDateTimeBestEffort({0})",
                [SqlServer] = $"CONVERT(DATETIME, {0}, 120)", // Using ODBC canonical format
                [MySql] = $"STR_TO_DATE({0}, '%Y-%m-%d %H:%i:%s')",
                [Oracle] = $"TO_DATE({0}, 'YYYY-MM-DD HH24:MI:SS')",
                [PostgreSql] = $"TO_TIMESTAMP({0}, 'YYYY-MM-DD HH24:MI:SS')"
            });


        Conversion(Unknown, Text)
            .Templates(new()
            {
                [PostgreSql] = $"({0}::text)",
                [SqlServer] = $"CAST({0} AS NVARCHAR(MAX))",
                [MySql] = $"CAST({0} AS CHAR)",
                [Oracle] = $"TO_CHAR({0})",
                [ClickHouse] = $"toString({0})",
            });
    }
}