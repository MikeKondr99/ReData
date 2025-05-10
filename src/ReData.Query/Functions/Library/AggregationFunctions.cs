using ReData.Query.Core.Types;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;
using static DataType;

public class AggregationFunctions : FunctionsDescriptor
{
    protected override void Functions()
    {
        int value = 0;
        
        Function("COUNT")
            .ReturnsNotNull(Integer)
            .Templates(new()
            {
                [All] = $"COUNT(*)"
            });
        
        foreach (var T in Types.AllWithoutBool)
        {
            
            Function("COUNT")
                .Arg("value", T)
                .ReturnsNotNull(Integer)
                .Templates(new()
                {
                    [All] = $"COUNT({value})"
                });
            
            
            Function("MIN")
                .Arg("value", T)
                .Returns(T)
                .CustomNullPropagation((_) => true)
                .Templates(new()
                {
                    [ClickHouse] = $"MIN(toNullable({value}))",
                    [All] = $"MIN({value})"
                });
            
            Function("MAX")
                .Arg("value", T)
                .Returns(T)
                .CustomNullPropagation((_) => true)
                .Templates(new()
                {
                    [ClickHouse] = $"MAX(toNullable({value}))",
                    [All] = $"MAX({value})"
                });
        }

        foreach (var T in new [] { Number, Integer, DateTime})
        {
            Function("COUNT_DISTINCT")
                .Arg("value", T)
                .ReturnsNotNull(Integer)
                .Templates(new()
                {
                    [All] = $"COUNT(DISTINCT {value})"
                });
            
            Function("ONLY")
                .Arg("value", T)
                .Returns(T)
                .CustomNullPropagation((_) => true)
                .Templates(new()
                {
                    [All] = $"(CASE WHEN COUNT(DISTINCT {value}) = 1 THEN MAX({value}) ELSE NULL END)"
                });
        }
        
        Function("COUNT_DISTINCT")
            .Arg("value", Text)
            .ReturnsNotNull(Integer)
            .Templates(new()
            {
                [SqlServer] = $"COUNT(DISTINCT {value} COLLATE sql_latin1_general_cp1_cs_as)",
                [MySql] = $"COUNT(DISTINCT BINARY {value})",
                [All] = $"COUNT(DISTINCT {value})"
            });
        
        Function("ONLY")
            .Arg("value", Text)
            .Returns(Text)
            .CustomNullPropagation((_) => true)
            .Templates(new()
            {
                [SqlServer] = $"(CASE WHEN COUNT(DISTINCT {value} COLLATE sql_latin1_general_cp1_cs_as) = 1 THEN MAX({value}) ELSE NULL END)",
                [MySql] = $"(CASE WHEN COUNT(DISTINCT BINARY {value}) = 1 THEN MAX({value}) ELSE NULL END)",
                [All] = $"(CASE WHEN COUNT(DISTINCT {value}) = 1 THEN MAX({value}) ELSE NULL END)"
            });

        foreach (var T in Types.Numbers)
        {
            Function("SUM")
                .Arg("value", T)
                .ReturnsNotNull(T)
                .Templates(new()
                {
                    [All] = $"COALESCE(SUM({value}), 0)"
                });
        }

        foreach (var T in Types.Numbers) 
        {
            Function("AVG")
                .Arg("value", T)
                .Returns(Number)
                .CustomNullPropagation(_ => true)
                .Templates(new()
                {
                    [ClickHouse] = $"CASE WHEN isNaN(AVG({value})) THEN NULL ELSE AVG({value}) END",
                    [All & ~ClickHouse] = $"AVG({value})"
                });
        }
        
        Function("AVG")
            .Arg("value", DateTime)
            .Returns(DateTime)
            .CustomNullPropagation(_ => true)
            .Templates(new()
            {
                [SqlServer] = $"DATEADD(ss, AVG(CAST(DATEDIFF(s,'1970-01-01 00:00:00', {value}) AS BIGINT)), '19700101')",
                [PostgreSql] = $"to_timestamp(AVG(EXTRACT(EPOCH FROM {value})))",
                [MySql] = $"FROM_UNIXTIME(AVG(UNIX_TIMESTAMP({value})))",
                [ClickHouse] = $"fromUnixTimestamp(CASE WHEN isNaN(AVG(toUnixTimestamp({value}))) THEN NULL ELSE toInt64(AVG(toUnixTimestamp({value}))) END)",
            });
        
        
        // Sum c условием
        // SUM(price, id > 10)
        // Function("SUM")
        //     .Arg("value", Number)
        //     .Arg("condition", Bool)
        //     .Returns(Number)
        //     .Templates(new()
        //     {
        //         [All] = $"SUM(CASE WHEN {1} THEN {0} ELSE NULL)",
        //     });

    }
}