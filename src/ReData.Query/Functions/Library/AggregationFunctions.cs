using System.Globalization;
using ReData.Query.Common;
using ReData.Query.Core.Template;
using ReData.Query.Core.Types;
using ReData.Query.Core.Value;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypes;
using static DataType;

public class AggregationFunctions : FunctionsDescriptor
{
    private static ITemplate FractileTemplate(DatabaseTypes database, TemplateContext context)
    {
        if (context.Arguments.Count <= 1)
        {
            throw new TemplateExprErrorException(new ExprError
            {
                Span = default,
                Message = "FRACTILE требует константный второй аргумент типа num.",
            });
        }

        var percentileArg = context.Arguments[1];
        var percentileNumber = GetConst<NumberValue>(percentileArg, context.Constants);
        var percentileInteger = GetConst<IntegerValue>(percentileArg, context.Constants);
        var percentileValue = percentileNumber?.Value ?? percentileInteger?.Value ??
            throw new TemplateExprErrorException(new ExprError
            {
                Span = percentileArg.Expression.Span,
                Message = "FRACTILE требует константный второй аргумент типа num.",
            });

        if (percentileValue < 0 || percentileValue > 1)
        {
            throw new TemplateExprErrorException(new ExprError
            {
                Span = percentileArg.Expression.Span,
                Message = "FRACTILE не поддерживает значения вне диапазона [0, 1].",
            });
        }

        var p = percentileValue.ToString(CultureInfo.InvariantCulture);
        return database switch
        {
            PostgreSql => Template.Create($"PERCENTILE_CONT({p}) WITHIN GROUP (ORDER BY {0})"),
            ClickHouse => Template.Create($"quantileExactInclusive({p})({0})"),
            // Не ожидается что мы сюда вообще попадём
            _ => throw new NotSupportedException($"FRACTILE не поддерживается для {database}"),
        };
    }

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

        foreach (var type in Types.AllWithoutBool)
        {
            AggFunction("COUNT")
                .Doc("Подсчитывает количество отличных от NULL значений в столбце @value")
                .Arg("value", type)
                .ReturnsNotNull(Integer)
                .Templates(new()
                {
                    [All] = $"COUNT({value})"
                });

            AggFunction("MIN")
                .Doc("Находит минимальное значение среди отличных от NULL значений в столбце @value")
                .Arg("value", type)
                .Returns(type)
                .CustomNullPropagation((_) => true)
                .Templates(new()
                {
                    [ClickHouse] = $"MIN(toNullable({value}))",
                    [All] = $"MIN({value})"
                });

            AggFunction("MAX")
                .Doc("Находит максимальное значение среди отличных от NULL значений в столбце x")
                .Arg("value", type)
                .Returns(type)
                .CustomNullPropagation((_) => true)
                .Templates(new()
                {
                    [ClickHouse] = $"MAX(toNullable({value}))",
                    [All] = $"MAX({value})"
                });

            AggFunction("MODE")
                .Doc("Возвращает наиболее часто встречающееся значение среди отличных от NULL значений в столбце @value. Поддержка: PostgreSql, ClickHouse")
                .Arg("value", type)
                .Returns(type)
                .CustomNullPropagation(_ => true)
                .Templates(new()
                {
                    [PostgreSql] = $"MODE() WITHIN GROUP (ORDER BY {value})",
                    [ClickHouse] = $"if(COUNT({value}) = 0, NULL, arrayElement(topK(1)({value}), 1))",
                });
        }

        foreach (var type in new[] { Number, Integer, DateTime })
        {
            AggFunction("COUNT_DISTINCT")
                .Doc("Подсчитывает количество уникальных отличных от NULL значений в столбце @value")
                .Arg("value", type)
                .ReturnsNotNull(Integer)
                .Templates(new()
                {
                    [All] = $"COUNT(DISTINCT {value})"
                });

            AggFunction("ONLY")
                .Doc("Возвращает значение, если оно уникально в наборе данных, иначе NULL")
                .Arg("value", type)
                .Returns(type)
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

        foreach (var type in Types.Numbers)
        {
            AggFunction("SUM")
                .Doc("Вычисляет сумму всех отличных от NULL значений в столбце @value")
                .Arg("value", type)
                .ReturnsNotNull(type)
                .Templates(new()
                {
                    [All] = $"COALESCE(SUM({value}), 0)"
                });
        }

        foreach (var type in Types.Numbers)
        {
            AggFunction("AVG")
                .Doc("Вычисляет среднее арифметическое отличных от NULL значений в столбце @value")
                .Arg("value", type)
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

        AggFunction("MEDIAN")
            .Doc("Возвращает медиану (непрерывную) среди отличных от NULL значений в столбце @value. Поддержка: PostgreSql, ClickHouse")
            .Arg("value", Number)
            .Returns(Number)
            .CustomNullPropagation(_ => true)
            .Templates(new()
            {
                [PostgreSql] = $"PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY {value})",
                [ClickHouse] = $"quantileExactInclusive(0.5)({value})",
            });

        AggFunction("FRACTILE")
            .Doc("Возвращает непрерывную квантиль для @value по константному параметру @p (0..1). Поддержка: PostgreSql, ClickHouse")
            .Arg("value", Number)
            .ReqArg("p", Number, isConst: true)
            .Returns(Number)
            .CustomNullPropagation(_ => true)
            .TemplatesDynamic(new Dictionary<DatabaseTypes, Func<TemplateContext, ITemplate>>()
            {
                [PostgreSql] = ctx => FractileTemplate(PostgreSql, ctx),
                [ClickHouse] = ctx => FractileTemplate(ClickHouse, ctx),
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

        foreach (var type in Types.AllWithoutBool)
        {
            AggFunction("CONCAT")
                .Doc("Aggregates values into a string with delimiter after sorting by specified column")
                .Arg("value", Text)
                .Arg("delimiter", Text)
                .Arg("sort", type)
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
    }
}
