using ReData.Query.Core.Types;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypes;
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
        };
        return Method(name)
            .Arg("input", input)
            .Returns(ret);
    }

    protected override void Functions()
    {
        // Noop Conversions
        foreach (var type in new DataType[]
                 {
                     Number, Integer, Text, Bool, DateTime
                 })
        {
            Conversion(type, type)
                .Doc("Не производит никаких действий")
                .Templates(new()
                {
                    [All] = $"{0}",
                });
        }

        Conversion(Text, Integer)
            .Doc("Преобразует текст в целое число")
            .Templates(new()
            {
                [SqlServer] = $"CAST({0} AS INTEGER)",
                [MySql] = $"CAST({0} AS SIGNED)",
                [PostgreSql | Oracle] = $"CAST({0} AS INTEGER)",
                [ClickHouse] = $"CAST({0} AS Int64)",
            });

        Conversion(Bool, Integer)
            .Doc("Преобразует логическое значение в целое число (Истина → 1, Ложь → 0)")
            .Templates(new()
            {
                [All] = $"CASE WHEN {0} THEN 1 ELSE 0 END",
            });

        Conversion(Number, Integer)
            .Doc("Преобразует число в целое число (отбрасывает дробную часть)")
            .Templates(new()
            {
                [SqlServer] = $"CAST({0} AS INTEGER)",
                [PostgreSql | Oracle] = $"CAST(FLOOR({0}) AS INTEGER)",
                [MySql] = $"CAST(FLOOR({0}) AS SIGNED)",
                [ClickHouse] = $"CAST({0} AS Int64)"
            });

        Conversion(Null, Integer)
            .Templates(new()
            {
                [All] = $"({0} + 0)",
            });

        Conversion(Text, Number)
            .Doc("Преобразует текст в число с плавающей точкой")
            .Templates(new()
            {
                [All & ~ClickHouse & ~Oracle] = $"CAST({0} AS DECIMAL(20,10))",
                [Oracle] = $"TO_NUMBER({0})",
                [ClickHouse] = $"toDecimal64({0}, 10)"
            });

        Conversion(Bool, Number)
            .Doc("Преобразует логическое значение в число (true → 1.0, false → 0.0)")
            .Templates(new()
            {
                [All] = $"CASE WHEN {0} THEN 1.0 ELSE 0.0 END"
            });

        Conversion(Integer, Number)
            .Doc("Преобразует целое число в число с плавающей точкой (добавляет .0)")
            .Templates(new()
            {
                [All & ~ClickHouse & ~Oracle] = $"CAST({0} AS DECIMAL(30,15))",
                [Oracle] = $"CAST({0} AS NUMERIC)",
                [ClickHouse] = $"toDecimal64({0}, 10)"
            });

        Conversion(Null, Number)
            .Templates(new()
            {
                [All] = $"({0} + 0.0)",
            });

        Conversion(Text, Bool)
            .Doc("Возвращает true если текст не пустой")
            .Templates(new()
            {
                [SqlServer] = $"LEN({0}) > 0",
                [MySql | PostgreSql | ClickHouse | Oracle] = $"LENGTH({0}) > 0"
            });


        Conversion(Number, Bool)
            .Doc("Возвращает true если дробное число больше нуля")
            .Templates(new()
            {
                [All] = $"({0} > 0.0)"
            });

        Conversion(Integer, Bool)
            .Doc("Возвращает true если целое число больше нуля")
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
            .Doc("Преобразует логическое значение в текст ('true' или 'false')")
            .Templates(new()
            {
                [SqlServer] = $"CASE WHEN {0} THEN N'true' ELSE N'false' END",
                [All] = $"CASE WHEN {0} THEN 'true' ELSE 'false' END"
            });

        Conversion(Number, Text)
            .Doc("Преобразует число в текстовое представление (например, 3.14 → '3.14')")
            .Templates(new()
            {
                [All & ~(MySql | Oracle)] = $"CAST({0} AS VARCHAR)",
                [MySql] = $"CAST({0} AS CHAR)",
                [Oracle] = $"REPLACE(TO_CHAR({0}),',','.')"
            });

        Conversion(Integer, Text)
            .Doc("Преобразует целое число в текстовое представление (например, 42 → '42')")
            .Templates(new()
            {
                [All & ~MySql & ~Oracle] = $"CAST({0} AS VARCHAR)",
                [MySql] = $"CAST({0} AS CHAR)",
                [Oracle] = $"TO_CHAR({0})"
            });

        Conversion(DateTime, Text)
            .Doc("Преобразует дату в текстовое представление в формате ISO")
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
            .Doc("Парсит строку как дату (формат ISO)")
            .Templates(new()
            {
                [ClickHouse] = $"parseDateTimeBestEffortOrNull({0})",
                [SqlServer] = $"CONVERT(DATETIME, {0}, 120)", // Using ODBC canonical format
                [MySql] = $"STR_TO_DATE({0}, '%Y-%m-%d %H:%i:%s')",
                [Oracle] = $"TO_DATE({0}, 'YYYY-MM-DD HH24:MI:SS')",
                [PostgreSql] = $"TO_TIMESTAMP({0}, 'YYYY-MM-DD HH24:MI:SS')"
            });


        Conversion(Unknown, Text)
            .Doc("Преобразует значение неизвестного типа в текстовое представление")
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
