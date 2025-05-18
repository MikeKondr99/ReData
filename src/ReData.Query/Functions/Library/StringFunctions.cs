using ReData.Query.Core.Types;
using ReData.Query.Lang.Expressions;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;
using static DataType;

public class StringFunctions : FunctionsDescriptor
{
    protected override void Functions()
    {
        int input = 0, start = 1, count = 2;

        Method("Substring")
            .Doc("Возвращает подстроку из текста, начиная с указанной позиции")
            .Arg("input", Text)
            .Arg("start", Integer)
            .Returns(Text)
            .Templates(new()
            {
                [PostgreSql | MySql] = $"SUBSTRING({input} FROM {start})",
                [SqlServer] = $"SUBSTRING({input}, {start}, LEN({input}) - ({start} - 1))",
                [ClickHouse] = $"SUBSTRING({input}, {start}, LENGTH({input}) - ({start} - 1))",
                [Oracle] = $"SUBSTR({input}, {start})",
            });
        
        Method("Substring")
            .Doc("Возвращает подстроку указанной длины, начиная с заданной позиции")
            .Arg("input", Text)
            .Arg("start", Integer)
            .Arg("count", Integer)
            .Returns(Text)
            .Templates(new()
            {
                [PostgreSql | MySql] = $"SUBSTRING({input} FROM {start} FOR {count})",
                [SqlServer | ClickHouse] = $"SUBSTRING({input}, {start}, {count})",
                [Oracle] = $"SUBSTR({input}, {start}, {count})"
            });

        Method("Lower")
            .Doc("Преобразует текст в нижний регистр")
            .Arg("input", Text)
            .Returns(Text)
            .Template($"LOWER({input})");
        
        Method("Upper")
            .Doc("Преобразует текст в верхний регистр")
            .Arg("input", Text)
            .Returns(Text)
            .Template($"UPPER({input})");
        
        Method("Trim")
            .Doc("Удаляет пробелы с обоих концов текста")
            .Arg("input", Text)
            .Returns(Text)
            .Template($"TRIM({input})");
        
        Method("TrimLeft")
            .Doc("Удаляет пробелы в начале текста")
            .Arg("input", Text)
            .Returns(Text)
            .Template($"LTRIM({input})");
        
        Method("TrimRight")
            .Doc("Удаляет пробелы в конце текста")
            .Arg("input", Text)
            .Returns(Text)
            .Template($"RTRIM({input})");
        
        Method("Reverse")
            .Doc("Возвращает текст в обратном порядке")
            .Arg("input", Text)
            .Returns(Text)
            .Template($"REVERSE({input})");

        Binary("+", Text, Text)
            .Doc("Объединяет две строки в одну")
            .Returns(Text)
            .Templates(new()
            {
                [SqlServer] = $"({0} + {1})",
                [PostgreSql] = $"({0} || {1})",
                [MySql | Oracle | ClickHouse] = $"CONCAT({0}, {1})",
            });
        
        Method("EmptyIsNull")
            .Doc("Преобразует пустую строку в NULL")
            .Arg("input", Text)
            .Returns(Text)
            .CustomNullPropagation(nulls => true)
            .Templates(new()
            {
                [Oracle] = $"{0}", // В Oracle пустая строка и null это одно и тоже
                [ClickHouse] = $"nullIf({0}, '')",
                [All] = $"NULLIF({0}, '')", 
            });
        
        Method("Replace")
            .Doc("Заменяет все вхождения подстроки на указанное значение")
            .Arg("input", Text)
            .Arg("from", Text)
            .Arg("to", Text)
            .Returns(Text)
            .Templates(new()
            {
                // Standard REPLACE (non-regex) for all databases
                [Oracle] = $"CASE WHEN {1} IS NULL OR {2} IS NULL THEN NULL ELSE REPLACE({input}, {1}, {2}) END",
                [All] = $"REPLACE({input}, {1}, {2})",
            });
        
        
         int substring = 1;
        Method("Index")
            .Arg("input", Text)
            .Arg("substring", Text)
            .Returns(Integer)
            .CustomNullPropagation(_ => true)
            .Templates(new()
            {
                [SqlServer] = $"NULLIF(CHARINDEX({input}, {substring}), 0)",
                [MySql] = $"NULLIF(LOCATE({substring}, {input}), 0)",
                [PostgreSql] = $"NULLIF(STRPOS({substring}, {input}), 0)",
                [Oracle] = $"NULLIF(INSTR({input}, {substring}), 0)",
                [ClickHouse] = $"nullIf(positionUTF8({input}, {substring}),0)",
            });

        Method("LastIndex")
            .Arg("input", Text)
            .Arg("substring", Text)
            .Returns(Integer)
            .CustomNullPropagation(_ => true)
            .Templates(new()
            {
                [Oracle] = $"(LENGTH({input}) - LENGTH({substring}) + 2 - NULLIF(INSTR(REVERSE({input}), REVERSE({substring})), 0))",
                [SqlServer] = $"(LEN({input}) - LEN({substring}) + 2 - NULLIF(CHARINDEX(REVERSE({substring}), REVERSE({input})), 0))",
                [ClickHouse] = $"(lengthUTF8({input}) - lengthUTF8({substring}) + 2 - nullIf(positionUTF8(reverseUTF8({input}), reverseUTF8({substring})), 0))",
                [MySql] = $"(CHAR_LENGTH({input}) - CHAR_LENGTH({substring}) + 2 - NULLIF(INSTR(REVERSE({input}), REVERSE({substring})), 0))",
                [PostgreSql] = $"(CHAR_LENGTH({input}) - CHAR_LENGTH({substring}) + 2 - NULLIF(STRPOS(REVERSE({input}), REVERSE({substring})), 0))",
            });


        Method("Len")
            .Arg("input", Text)
            .Returns(Integer)
            .Templates(new()
            {
                [SqlServer] = $"LEN({input} + N'.') - 1",
                [MySql] = $"CHAR_LENGTH({input})",
                [PostgreSql] = $"CHAR_LENGTH({input})",
                [Oracle] = $"LENGTH({input})",
                [ClickHouse] = $"lengthUTF8({input})",
            });

        int delimiter = 1, position = 2;
        Method("Split")
            .Arg("input", Text)
            .Arg("delimeter", Text)
            .Arg("position", Integer)
            .Returns(Text)
            .CustomNullPropagation(_ => true)
            .Templates(new()
            {
                [SqlServer] = $"(select value from STRING_SPLIT(REPLACE({input},{delimiter}, 'ඞ'), 'ඞ') order by (select 1) offset {position} - 1 rows fetch next 1 row only)",
                [PostgreSql] = $"SPLIT_PART({input}, {delimiter}, {position})",
                [MySql] = $"""
                           REPLACE(
                                    SUBSTRING(
                                         SUBSTRING_INDEX({input}, {delimiter}, {position}),
                                         CHAR_LENGTH(
                                              SUBSTRING_INDEX({input}, {delimiter}, {position} - 1)
                                         ) + 1
                                    ),
                                    {delimiter},
                                    ''
                               )
                           """,
                [ClickHouse] = $"arrayElement(splitByString({delimiter}, {input}), {position})",
            });
    }

}