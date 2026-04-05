using ReData.Query.Core.Types;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypes;
using static DataType;

public class NumberFunctions : FunctionsDescriptor
{
    protected override void Functions()
    {
        int input = 0, step = 1, offset = 2;
        int modulus = 1;
        Method("Mod")
            .Doc("Возвращает остаток от деления (знак результата соответствует знаку делимого)")
            .Arg("input", Integer)
            .ReqArg("modulus", Integer)
            .Returns(Integer)
            .Templates(new()
            {
                [All & ~SqlServer] = $"MOD({input}, {modulus})",
                [SqlServer] = $"({input} % {modulus})",
            });

        Method("Rem")
            .Doc("Возвращает остаток от деления (знак результата соответствует знаку делителя)")
            .Arg("input", Integer)
            .Arg("modulus", Integer)
            .Returns(Integer)
            .Templates(new()
            {
                [All & ~SqlServer] = $"ABS(MOD({input}, {modulus}))",
                [SqlServer] = $"ABS({input} % {modulus})"
            });

        foreach (var type in new[]
                 {
                     Integer, Number
                 })
        {
            Method("Abs")
                .Doc("Возвращает абсолютное значение")
                .Arg("input", type)
                .Returns(type)
                .Templates(new()
                {
                    [All] = $"ABS({input})"
                });
        }

        Method("Floor")
            .Doc("Округляет число вниз до ближайшего целого")
            .Arg("input", Number)
            .Returns(Number)
            .Template($"FLOOR({input})");

        Method("Ceil")
            .Doc("Округляет число вверх до ближайшего целого")
            .Arg("input", Number)
            .Returns(Number)
            .Templates(new()
            {
                [All & ~SqlServer] = $"CEIL({input})",
                [SqlServer] = $"CEILING({input})"
            });

        Method("Round")
            .Doc("Округляет число до ближайшего целого (по правилам математического округления)")
            .Arg("input", Number)
            .Returns(Number)
            .Templates(new()
            {
                [All & ~SqlServer & ~ClickHouse] = $"ROUND({input})",
                [SqlServer] = $"ROUND(CAST({input} AS NUMERIC),0)",
                [ClickHouse] = $"ROUND(CAST({input},'Decimal64(6)'),0)"
            });

        Method("Floor")
            .Doc("Округляет число вниз с заданным шагом")
            .Arg("input", Number)
            .Arg("step", Number)
            .Returns(Number)
            .Templates(new()
            {
                [All] = $"FLOOR({input} / {step}) * {step}",
            });

        Method("Ceil")
            .Doc("Округляет число вверх с заданным шагом")
            .Arg("input", Number)
            .Arg("step", Number)
            .Returns(Number)
            .Templates(new()
            {
                [All & ~Oracle] = $"CEILING({input} / {step}) * {step}",
                [Oracle] = $"CEIL({input} / {step}) * {step}"
            });

        Method("Round")
            .Doc("Округляет число до ближайшего кратного заданному шагу")
            .Arg("input", Number)
            .Arg("step", Number)
            .Returns(Number)
            .Templates(new()
            {
                [All & ~ SqlServer & ~ ClickHouse] = $"ROUND({input} / {step}) * {step}",
                [SqlServer] = $"ROUND({input} / {step}, 0) * {step}",
                [ClickHouse] = $"ROUND(CAST({input} / {step},'Decimal64(6)')) * {step}",
            });

        Method("Floor")
            .Doc("Округляет число вниз с заданным шагом и смещением")
            .Arg("input", Number)
            .Arg("step", Number)
            .Arg("offset", Number)
            .Returns(Number)
            .Templates(new()
            {
                [All] = $"FLOOR(({input} - {offset}) / {step}) * {step} + {offset}",
            });

        Method("Ceil")
            .Doc("Округляет число вверх с заданным шагом и смещением")
            .Arg("input", Number)
            .Arg("step", Number)
            .Arg("offset", Number)
            .Returns(Number)
            .Templates(new()
            {
                [All & ~Oracle] = $"CEILING(({input} - {offset}) / {step}) * {step} + {offset}",
                [Oracle] = $"CEIL(({input} - {offset}) / {step}) * {step} + {offset}",
            });

        Method("Round")
            .Doc("Округляет число с заданным шагом и смещением")
            .Arg("input", Number)
            .Arg("step", Number)
            .Arg("offset", Number)
            .Returns(Number)
            .Templates(new()
            {
                [All & ~SqlServer & ~ClickHouse] = $"Round(({input} - {offset}) / {step}) * {step} + {offset}",
                [SqlServer] = $"Round(({input} - {offset}) / {step}, 0) * {step} + {offset}",
                [ClickHouse] = $"Round(CAST(({input} - {offset}) / {step},'Decimal64(6)')) * {step} + {offset}",
            });

        Method("Even")
            .Doc("Проверяет, является ли число чётным")
            .Arg("input", Integer)
            .Returns(Bool)
            .Templates(new()
            {
                [All & ~SqlServer] = $"(MOD({input}, 2) = 0)",
                [SqlServer] = $"(({input} % 2) = 0)",
            });

        Method("Odd")
            .Doc("Проверяет, является ли число нечётным")
            .Arg("input", Integer)
            .Returns(Bool)
            .Templates(new()
            {
                [All & ~SqlServer] = $"(MOD({input}, 2) <> 0)",
                [SqlServer] = $"(({input} % 2) <> 0)",
            });

        foreach (var type in new[]
                 {
                     Integer, Number
                 })
        {
            Method("Sign")
                .Doc("Возвращает знак числа (-1, 0 или 1)")
                .Arg("input", type)
                .Returns(Integer)
                .Templates(new()
                {
                    [All] = $"SIGN({input})",
                });
        }

        Method("Frac")
            .Doc("Возвращает дробную часть числа")
            .Arg("input", Number)
            .Returns(Number)
            .Templates(new()
            {
                [All & ~SqlServer] = $"MOD({input}, 1)", // [ "MOD(", arg0, ", 1)" ]
                [SqlServer] = $"({input} % 1)",
            });
    }
}