using System;
using System.Collections.Generic;
using System.Linq;

using ReData.Query.Core.Types;
using ReData.Query.Core.Template;
using ReData.Query.Core.Value;

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

    private static ITemplate NullTemplate() => Template.Create("NULL");

    private static ITemplate FieldTemplate(DatabaseTypes database, TemplateContext context)
    {
        if (context.Arguments.Count == 0 || context.Arguments[0] is null)
        {
            throw new InvalidOperationException("Const argument is missing.");
        }

        var arg = context.Arguments[0]!;
        if (arg is not IntegerValue(var value))
        {
            throw new InvalidOperationException("Field expects integer index.");
        }

        if (value <= 0 || value > context.Fields.Count)
        {
            return NullTemplate();
        }

        var field = context.Fields[(int)value - 1];
        return TextTemplate(database, field.Type.Type, field.Template);
    }

    private static ITemplate FieldTemplateByName(DatabaseTypes database, TemplateContext context)
    {
        if (context.Arguments.Count == 0 || context.Arguments[0] is null)
        {
            throw new InvalidOperationException("Const argument is missing.");
        }

        var arg = context.Arguments[0]!;
        if (arg is not TextValue(var value))
        {
            throw new InvalidOperationException("Field expects text name.");
        }

        var field = context.Fields.FirstOrDefault(f => f.Alias == value);
        if (string.IsNullOrEmpty(field.Alias))
        {
            return NullTemplate();
        }

        return TextTemplate(database, field.Type.Type, field.Template);
    }

    private static ITemplate TextTemplate(DatabaseTypes database, DataType type, ITemplate input)
    {
        if (type is Text)
        {
            return input;
        }
        
        TemplateInterpolatedStringHandler template = type switch
        {
            Bool => database switch
            {
                SqlServer => $"CASE WHEN {input} THEN N'true' ELSE N'false' END",
                PostgreSql or MySql or ClickHouse or Oracle => $"CASE WHEN {input} THEN 'true' ELSE 'false' END",
            },
            Number => database switch
            {
                MySql => $"CAST({input} AS CHAR)",
                Oracle => $"REPLACE(TO_CHAR({input}),',','.')",
                PostgreSql or SqlServer or ClickHouse => $"CAST({input} AS VARCHAR)"
            },
            Integer => database switch
            {
                MySql => $"CAST({input} AS CHAR)",
                Oracle => $"TO_CHAR({input})",
                PostgreSql or SqlServer or ClickHouse => $"CAST({input} AS VARCHAR)"
            },
            DateTime => database switch
            {
                SqlServer => $"CONVERT(NVARCHAR(20), {input}, 120)",
                ClickHouse => $"formatDateTime({input}, '%Y-%m-%d %H:%i:%S')",
                MySql => $"DATE_FORMAT({input}, '%Y-%m-%d %H:%i:%S')",
                PostgreSql => $"TO_CHAR({input}, 'YYYY-MM-DD HH24:MI:SS')",
                Oracle => $"TO_CHAR({input}, 'YYYY-MM-DD HH24:MI:SS')",
            },
            Null => $"LOWER({input})",
            Unknown => database switch
            {
                PostgreSql => $"({input}::text)",
                SqlServer => $"CAST({input} AS NVARCHAR(MAX))",
                MySql => $"CAST({input} AS CHAR)",
                Oracle => $"TO_CHAR({input})",
                ClickHouse => $"toString({input})",
                _ => $"{input}",
            },
            _ => throw new NotSupportedException($"type: {type}, database: {database}"),
        };
        return Template.Create(template);
    }

    protected override void Functions()
    {
        foreach (var type in new[] { Text, Number, Integer, Bool, Null, DateTime, Unknown })
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

        Method("Field")
            .Doc("Возвращает значение поля по индексу и приводит к тексту")
            .ReqArg("input", Integer, isConst: true)
            .Returns(Text)
            .CustomNullPropagation(_ => true)
            .TemplatesDynamic(new Dictionary<DatabaseTypes, Func<TemplateContext, ITemplate>>()
            {
                [SqlServer] = ctx => FieldTemplate(SqlServer, ctx),
                [MySql] = ctx => FieldTemplate(MySql, ctx),
                [PostgreSql] = ctx => FieldTemplate(PostgreSql, ctx),
                [Oracle] = ctx => FieldTemplate(Oracle, ctx),
                [ClickHouse] = ctx => FieldTemplate(ClickHouse, ctx),
            });

        Method("Field")
            .Doc("Возвращает значение поля по имени и приводит к тексту")
            .ReqArg("input", Text, isConst: true)
            .Returns(Text)
            .CustomNullPropagation(_ => true)
            .TemplatesDynamic(new Dictionary<DatabaseTypes, Func<TemplateContext, ITemplate>>()
            {
                [SqlServer] = ctx => FieldTemplateByName(SqlServer, ctx),
                [MySql] = ctx => FieldTemplateByName(MySql, ctx),
                [PostgreSql] = ctx => FieldTemplateByName(PostgreSql, ctx),
                [Oracle] = ctx => FieldTemplateByName(Oracle, ctx),
                [ClickHouse] = ctx => FieldTemplateByName(ClickHouse, ctx),
            });

        Function("DbName")
            .Doc("Возвращает название текущей используемой внутри базы данных")
            .Returns(Text, ConstPropagation.AlwaysTrue)
            .Templates(new()
            {
                [SqlServer] = $"N'SqlServer'",
                [MySql] = $"'MySql'",
                [PostgreSql] = $"'PostgreSql'",
                [Oracle] = $"'Oracle'",
                [ClickHouse] = $"'ClickHouse'",
            });

        Function("DbVersion")
            .Doc("Возвращает версию текущей используемой внутри базы данных")
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
