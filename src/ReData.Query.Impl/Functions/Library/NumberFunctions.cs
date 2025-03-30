namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;
using static DataType;

public class NumberFunctions : FunctionsDescriptor
{
    protected override void Functions()
    {
        int input = 0, step = 1, offset = 2;
        int modulus = 1;
        Method("Mod")
            .Arg("input", Integer)
            .ReqArg("modulus", Integer)
            .Returns(Integer)
            .Templates(new()
            {
                [All & ~SqlServer] = $"MOD({input}, {modulus})",
                [SqlServer] = $"({input} % {modulus})",
            });

        Method("Rem")
            .Arg("input", Integer)
            .Arg("modulus", Integer)
            .Returns(Integer)
            .Templates(new()
            {
                [All & ~SqlServer] = $"ABS(MOD({input}, {modulus}))",
                [SqlServer] = $"ABS({input} % {modulus})"
            });

        foreach (var T in new[]
                 {
                     Integer, Number
                 })
        {
            Function("Abs")
                .Arg("input", T)
                .Returns(T)
                .Templates(new()
                {
                    [All] = $"ABS({input})"
                });
        }

        Function("Floor")
            .Arg("input", Number)
            .Returns(Number)
            .Template($"FLOOR({input})");

        Function("Ceil")
            .Arg("input", Number)
            .Returns(Number)
            .Templates(new()
            {
                [All & ~SqlServer] = $"CEIL({input})",
                [SqlServer] = $"CEILING({input})"
            });

        Function("Round")
            .Arg("input", Number)
            .Returns(Number)
            .Templates(new()
            {
                [All & ~ SqlServer & ~ ClickHouse] = $"ROUND({input})",
                [SqlServer] = $"ROUND(CAST({input} AS NUMERIC),0)",
                [ClickHouse] = $"ROUND(CAST({input},'Decimal64(6)'),0)"
            });

        Function("Floor")
            .Arg("input", Number)
            .Arg("step", Number)
            .Returns(Number)
            .Templates(new()
            {
                [All] = $"FLOOR({input} / {step}) * {step}",
            });

        Function("Ceil")
            .Arg("input", Number)
            .Arg("step", Number)
            .Returns(Number)
            .Templates(new()
            {
                [All & ~Oracle] = $"CEILING({input} / {step}) * {step}",
                [Oracle] = $"CEIL({input} / {step}) * {step}"
            });

        Function("Round")
            .Arg("input", Number)
            .Arg("step", Number)
            .Returns(Number)
            .Templates(new()
            {
                [All & ~ SqlServer & ~ ClickHouse] = $"ROUND({input} / {step}) * {step}",
                [SqlServer] = $"ROUND({input} / {step}, 0) * {step}",
                [ClickHouse] = $"ROUND(CAST({input} / {step},'Decimal64(6)')) * {step}",
            });

        Function("Floor")
            .Arg("input", Number)
            .Arg("step", Number)
            .Arg("offset", Number)
            .Returns(Number)
            .Templates(new()
            {
                [All] = $"FLOOR(({input} - {offset}) / {step}) * {step} + {offset}",
            });

        Function("Ceil")
            .Arg("input", Number)
            .Arg("step", Number)
            .Arg("offset", Number)
            .Returns(Number)
            .Templates(new()
            {
                [All & ~Oracle] = $"CEILING(({input} - {offset}) / {step}) * {step} + {offset}",
                [Oracle] = $"CEIL(({input} - {offset}) / {step}) * {step} + {offset}",
            });

        Function("Round")
            .Arg("input", Number)
            .Arg("step", Number)
            .Arg("offset", Number)
            .Returns(Number)
            .Templates(new()
            {
                [All & ~ SqlServer & ~ ClickHouse] = $"Round(({input} - {offset}) / {step}) * {step} + {offset}",
                [SqlServer] = $"Round(({input} - {offset}) / {step}, 0) * {step} + {offset}",
                [ClickHouse] = $"Round(CAST(({input} - {offset}) / {step},'Decimal64(6)')) * {step} + {offset}",
            });

        Function("Even")
            .Arg("input", Integer)
            .Returns(Bool)
            .Templates(new()
            {
                [All & ~SqlServer] = $"(MOD({input}, 2) = 0)",
                [SqlServer] = $"(({input} % 2) = 0)",
            });

        Function("Odd")
            .Arg("input", Integer)
            .Returns(Bool)
            .Templates(new()
            {
                [All & ~SqlServer] = $"(MOD({input}, 2) <> 0)",
                [SqlServer] = $"(({input} % 2) <> 0)",
            });

        foreach (var T in new[]
                 {
                     Integer, Number
                 })
        {
            Method("Sign")
                .Arg("input", T)
                .Returns(Integer)
                .Templates(new()
                {
                    [All] = $"SIGN({input})",
                });
        }

        Method("Frac")
            .Arg("input", Number)
            .Returns(Bool)
            .Templates(new()
            {
                [All & ~SqlServer] = $"MOD({input}, 1)",
                [SqlServer] = $"({input} % 1)",
            });
    }
}