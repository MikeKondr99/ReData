using ReData.Query.Core.Types;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;
using static DataType;

public class ReflectionFunctions : FunctionsDescriptor
{
    private string TypeMapping(DataType type) => type switch
    {
        Text => "Text",
        Number => "Num",
        Integer => "Int",
        Bool => "Bool",
        DateTime => "Date",
        Unknown => "Unknown",
        Null => "Null",
    };
    
    protected override void Functions()
    {
        foreach (var T in new [] { Text, Number, Integer, Bool, Null, DateTime, Unknown })
        {
            Method("Type")
                .Doc("Возвращает тип значения в виде строки")
                .Arg("input", T)
                .ReturnsNotNull(Text, ConstPropagation.AlwaysTrue)
                .Templates(new()
                {
                    [All] = $"'{TypeMapping(T)}?'",
                });
            
            Method("Type")
                .Doc("Возвращает тип значения в виде строки")
                .ReqArg("input", T)
                .ReturnsNotNull(Text, ConstPropagation.AlwaysTrue)
                .Templates(new()
                {
                    [All] = $"'{TypeMapping(T)}'",
                });
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