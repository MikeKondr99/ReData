using ReData.Query.Core.Types;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;
using static DataType;

public class AggregationFunctions : FunctionsDescriptor
{
    protected override void Functions()
    {
        int value = 0;
        
        AggFunction("COUNT")
            .Doc("Возвращает общее количество строк в наборе данных (включая NULL-значения)")
            .ReturnsNotNull(Integer)
            .Templates(new()
            {
                [All] = $"COUNT(*)"
            });
        
        foreach (var T in Types.AllWithoutBool)
        {
            
            AggFunction("COUNT")
                .Doc("Подсчитывает количество отличных от NULL значений в столбце @value")
                .Arg("value", T)
                .ReturnsNotNull(Integer)
                .Templates(new()
                {
                    [All] = $"COUNT({value})"
                });
            
            
            AggFunction("MIN")
                .Doc("Находит минимальное значение среди отличных от NULL значений в столбце @value")
                .Arg("value", T)
                .Returns(T)
                .CustomNullPropagation((_) => true)
                .Templates(new()
                {
                    [ClickHouse] = $"MIN(toNullable({value}))",
                    [All] = $"MIN({value})"
                });
            
            AggFunction("MAX")
                .Doc("Находит максимальное значение среди отличных от NULL значений в столбце x")
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
            AggFunction("COUNT_DISTINCT")
                .Doc("Подсчитывает количество уникальных отличных от NULL значений в столбце @value")
                .Arg("value", T)
                .ReturnsNotNull(Integer)
                .Templates(new()
                {
                    [All] = $"COUNT(DISTINCT {value})"
                });
            
            AggFunction("ONLY")
                .Doc("Возвращает значение, если оно уникально в наборе данных, иначе NULL")
                .Arg("value", T)
                .Returns(T)
                .CustomNullPropagation((_) => true)
                .Templates(new()
                {
                    [All] = $"(CASE WHEN COUNT(DISTINCT {value}) = 1 THEN MAX({value}) ELSE NULL END)"
                });
        }
        
        AggFunction("COUNT_DISTINCT")
            .Doc("Подсчитывает количество уникальных отличных от NULL значений в столбце @value (Чувствительно к регистру)")
            .Arg("value", Text)
            .ReturnsNotNull(Integer)
            .Templates(new()
            {
                [SqlServer] = $"COUNT(DISTINCT {value} COLLATE sql_latin1_general_cp1_cs_as)",
                [MySql] = $"COUNT(DISTINCT BINARY {value})",
                [All] = $"COUNT(DISTINCT {value})"
            });
        
        AggFunction("ONLY")
            .Doc("Возвращает значение, если оно уникально в наборе данных, иначе NULL (Чувствительно к регистру)")
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
            AggFunction("SUM")
                .Doc("Вычисляет сумму всех отличных от NULL значений в столбце @value")
                .Arg("value", T)
                .ReturnsNotNull(T)
                .Templates(new()
                {
                    [All] = $"COALESCE(SUM({value}), 0)"
                });
        }

        foreach (var T in Types.Numbers) 
        {
            AggFunction("AVG")
                .Doc("Вычисляет среднее арифметическое отличных от NULL значений в столбце @value")
                .Arg("value", T)
                .Returns(Number)
                .CustomNullPropagation(_ => true)
                .Templates(new()
                {
                    [ClickHouse] = $"CASE WHEN isNaN(AVG({value})) THEN NULL ELSE AVG({value}) END",
                    [All & ~ClickHouse] = $"AVG({value})"
                });
        }
        
        AggFunction("AVG")
            .Doc("Вычисляет среднюю дату с точностью в секунду")
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

        int delimiter = 1, sort = 2;
        AggFunction("CONCAT")
            .Doc("Aggregates all non-NULL values into a single string without delimiter")
            .Arg("value", Text)
            .Returns(Text)
            .CustomNullPropagation(_ => true)
            .Templates(new()
            {
                [SqlServer] = $"STRING_AGG({value}, '')",
                [MySql] = $"GROUP_CONCAT({value} SEPARATOR '')",
                [PostgreSql] = $"STRING_AGG({value}, '')",
                [Oracle] = $"LISTAGG({value}, '') WITHIN GROUP (ORDER BY 1)",
                [ClickHouse] = $"if(empty(groupArray({value})), NULL, arrayStringConcat(groupArray({value}), ''))",
            });
        
        AggFunction("CONCAT")
            .Doc("Aggregates all non-NULL values into a single string with delimiter")
            .Arg("value", Text)
            .Arg("delimiter", Text)
            .Returns(Text)
            .CustomNullPropagation(_ => true)
            .Templates(new()
            {
                [SqlServer] = $"STRING_AGG({value}, {delimiter})",
                [MySql] = $"GROUP_CONCAT({value} SEPARATOR {delimiter})",
                [PostgreSql] = $"STRING_AGG({value}, {delimiter})",
                [Oracle] = $"LISTAGG({value}, {delimiter}) WITHIN GROUP (ORDER BY 1)",
                [ClickHouse] = $"if(empty(groupArray({value})), NULL, arrayStringConcat(groupArray({value}), {delimiter}))",
            });

        foreach (var T in Types.AllWithoutBool)
        {
            AggFunction("CONCAT")
                .Doc("Aggregates values into a string with delimiter after sorting by specified column")
                .Arg("value", Text)
                .Arg("delimiter", Text)
                .Arg("sort", T) // Sorting column (can be different from value)
                .Returns(Text)
                .CustomNullPropagation(_ => true)
                .Templates(new()
                {
                    [SqlServer] = $"STRING_AGG({value}, {delimiter}) WITHIN GROUP (ORDER BY {sort})",
                    [MySql] = $"GROUP_CONCAT({value} ORDER BY {sort} SEPARATOR {delimiter})",
                    [PostgreSql] = $"STRING_AGG({value}, {delimiter} ORDER BY {sort})",
                    [Oracle] = $"LISTAGG({value}, {delimiter}) WITHIN GROUP (ORDER BY {sort})",
                    [ClickHouse] = $"""
                                     if(empty(groupArray({value})), NULL,
                                     arrayStringConcat(
                                         arrayMap(
                                             x -> x.1,
                                             arraySort(
                                                 x -> x.2,
                                                 groupArray(({value}, {sort}))
                                             )
                                         ),
                                         {delimiter}
                                     ))
                                     """,
                    
                });
        }
        
        foreach (var T in Types.AllWithoutBool)
        {
            AggFunction("PERCENTILE")
                .ReqArg("level", Number, FunctionArgumentOptions.ConstOnly)
                .Arg("value", T)
                .Returns(T)
                .CustomNullPropagation(_ => true)
                .Templates(new()
                {
                    [PostgreSql] = $"PERCENTILE_DISC({0}) WITHIN GROUP (ORDER BY {1})"
                });
        }
    }
}