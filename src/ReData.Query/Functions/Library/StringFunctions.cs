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
                [All] = $"CASE WHEN {0} = '' THEN NULL ELSE {0} END", 
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
                [All] = $"REPLACE({input}, {1}, {2})",
            });
    }

}