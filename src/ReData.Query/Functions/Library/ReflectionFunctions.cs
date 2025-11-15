using ReData.Query.Core.Types;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypes;
using static DataType;

public class ReflectionFunctions : FunctionsDescriptor
{
    private static string TypeMapping(DataType type) => type switch
    {
        Text => "text",
        Number => "num",
        Integer => "int",
        Bool => "bool",
        DateTime => "date",
        Unknown => "unk",
        Null => "null",
    };
    
    protected override void Functions()
    {
        foreach (var type in new [] { Text, Number, Integer, Bool, Null, DateTime, Unknown })
        {
            Method("Type")
                .Doc("Возвращает тип значения в виде строки")
                .Arg("input", type)
                .ReturnsNotNull(Text, ConstPropagation.AlwaysTrue)
                .Templates(new()
                {
                    [All] = $"'{TypeMapping(type)}'",
                });

            // null! не существует и нет смысла для такой функции
            if (type is not Null)
            {
                Method("Type")
                    .Doc("Возвращает тип значения в виде строки")
                    .ReqArg("input", type)
                    .ReturnsNotNull(Text, ConstPropagation.AlwaysTrue)
                    .Templates(new()
                    {
                        [All] = $"'{TypeMapping(type)}!'",
                    });
            }
        }


        Function("DbVersion")
            .Returns(Text)
            .Templates(new()
            {
                [SqlServer] = $"SERVERPROPERTY('ProductVersion')",
                [MySql] = $"VERSION()",
                [PostgreSql] = $"(string_to_array(VERSION(), ' '))[2]",
                [Oracle] = $"""
                            REGEXP_SUBSTR(
                                (SELECT BANNER FROM v$version WHERE ROWNUM = 1),
                                'Release ([0-9.]+)',
                                1, 1, NULL, 1
                            )
                            """,
                [ClickHouse] = $"version()",
            });
    }
}