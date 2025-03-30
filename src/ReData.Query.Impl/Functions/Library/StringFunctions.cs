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
            .Doc("Retrieves a substring from input. The substring starts at a specified character position and continues to the end of the string.")
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
            .Doc("Retrieves a substring from `input`. The substring starts at a specified character position and has a specified length.")
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
            .Arg("input", Text)
            .Returns(Text)
            .Template($"LOWER({input})");
        
        Method("Upper")
            .Arg("input", Text)
            .Returns(Text)
            .Template($"UPPER({input})");
        
        Method("Trim")
            .Arg("input", Text)
            .Returns(Text)
            .Template($"TRIM({input})");
        
        Method("TrimLeft")
            .Arg("input", Text)
            .Returns(Text)
            .Template($"LTRIM({input})");
        
        Method("TrimRight")
            .Arg("input", Text)
            .Returns(Text)
            .Template($"RTRIM({input})");
        
        Method("Reverse")
            .Arg("input", Text)
            .Returns(Text)
            .Template($"REVERSE({input})");

        Binary("+", Text, Text)
            .Returns(Text)
            .Templates(new()
            {
                [SqlServer] = $"({0} + {1})",
                [PostgreSql] = $"({0} || {1})",
                [MySql | Oracle | ClickHouse] = $"CONCAT({0}, {1})",
            });
        
        
        // NullIf = (_) => true,
        Method("EmptyIsNull")
            .Arg("input", Text)
            .Returns(Text)
            .CustomNullPropagation(nulls => true)
            .Templates(new()
            {
                [All] = $"CASE WHEN {0} = '' THEN NULL ELSE {0} END", 
            });
        
        Method("Len")
            .Arg("input", Text)
            .Returns(Integer)
            .Templates(new()
            {
                [All] = $"LENGTH({input})", // Works in most databases
                [SqlServer] = $"LEN({input})" // SQL Server uses LEN
            });
        
        Function("Chr")
            .Arg("code", Integer)
            .Returns(Text)
            .Templates(new()
            {
                [PostgreSql] = $"CHR({0})",
                [MySql] = $"CHAR({0})",
                [SqlServer] = $"CHAR({0})",
                [Oracle] = $"CHR({0})",
                [ClickHouse] = $"char({0})"
            });
        
        Method("Ord")
            .Arg("input", Text)
            .Returns(Integer)
            .Templates(new()
            {
                [PostgreSql | MySql] = $"ASCII(SUBSTRING({input}, 1, 1))",
                [SqlServer] = $"ASCII(SUBSTRING({input}, 1, 1))",
                [Oracle] = $"ASCII(SUBSTR({input}, 1, 1))",
                [ClickHouse] = $"ASCII(substring({input}, 1, 1))"
            });
        
        Method("Replace")
            .Arg("input", Text)
            .Arg("from", Text)
            .Arg("to", Text)
            .Returns(Text)
            .Templates(new()
            {
                // Standard REPLACE (non-regex) for all databases
                [All] = $"REPLACE({input}, {1}, {2})",
                // Special case for Oracle to avoid regex interpretation
                [Oracle] = $"REGEXP_REPLACE({input}, REGEXP_REPLACE({1}, '([][)(}}{{.+*?^$\\])', '\\\\1'), {1}, 1, 0, 'i')"
                });

        Method("Repeat")
            .Arg("input", Text)
            .Arg("count", Integer)
            .Returns(Text)
            .Templates(new()
            {
                [SqlServer] = $"REPLICATE({input}, CASE WHEN {count} < 0 THEN 0 ELSE {count} END)",
                [MySql] = $"REPEAT({input}, GREATEST({count}, 0))",
                [PostgreSql] = $"REPEAT({input}, GREATEST({count}, 0))",
                [Oracle] = $"RPAD('', {count} * LENGTH({input}), {input})",
                [ClickHouse] = $"repeat({input}, greatest({count}, 0))"
            });
    }

}