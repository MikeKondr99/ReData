using System.Diagnostics;
using ReData.Query.Core.Template;
using ReData.Query.Core.Types;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypes;
using static DataType;

public class MathFunctions : FunctionsDescriptor
{
    protected override void Functions()
    {
        foreach (var t in new [] { Integer, Number })
        {
            var prefix2 = t switch
            {
                Integer => "целых",
                Number => "дробных"
            };
            var prefix = t switch
            {
                Integer => "целого",
                Number => "дробного"
            };
            
            Binary("+", t, t)
                .Doc($"Сложение двух {prefix2} чисел")
                .Returns(t)
                .Templates(new()
                {
                    [All] = $"({0} + {1})",
                });
            
            Binary("-", t, t)
                .Doc($"Вычитание двух {prefix2} чисел")
                .Returns(t)
                .Templates(new()
                {
                    [All] = $"({0} - {1})",
                });

            Unary("-")
                .Doc($"Унарный минус {prefix} числа")
                .Arg("value", t)
                .Returns(t)
                .Templates(new()
                {
                    [All] = $"(-{0})",
                });
            
            Binary("*", t, t)
                .Doc($"Умножение двух {prefix2} чисел")
                .Returns(t)
                .Templates(new()
                {
                    [All] = $"({0} * {1})", 
                });
        }

        Binary("/", Number, Number)
            .Doc("Деление дробных чисел")
            .Returns(Number)
            .CustomNullPropagation(_ => true)
            .Templates(new()
            {
                [PostgreSql | SqlServer | Oracle] = $"({0} / NULLIF({1}, 0))",
                [MySql] = $"({0} / NULLIF({1}, 0))",
                [ClickHouse] = $"({0} / nullIf({1}, 0))",
            });
        
        Binary("/", Integer, Integer)
            .Doc("Целочисленное деление (с отбрасыванием остатка)")
            .Returns(Integer)
            .CustomNullPropagation(_ => true)
            .Templates(new()
            {
                [PostgreSql | SqlServer] = $"(CAST({0} AS BIGINT) / NULLIF(CAST({1} AS BIGINT), 0))",
                [MySql] = $"(CAST({0} AS SIGNED) DIV NULLIF(CAST({1} AS SIGNED), 0))",
                [Oracle] = $"TRUNC({0} / NULLIF({1}, 0))",
                [ClickHouse] = $"intDiv({0}, nullIf({1}, 0))" 
            });

        Dictionary<DatabaseTypes, TemplateInterpolatedStringHandler> powTemplates = new()
        {
            [All] = $"POWER({0}, {1})",
        };

       // TODO: Not Tested
        Method("Pow")
            .Doc("Возведение числа в степень")
            .Arg("left", Number)
            .Arg("right", Number)
            .Returns(Number)
            .Templates(powTemplates);
        
       // TODO: Not Tested
       Binary("^", Number, Number)
            .Doc("Возведение числа в степень (альтернативный синтаксис)")
            .Returns(Number)
            .Templates(powTemplates);
       
       Function("E")
           .Doc("Возвращает математическую константу (e ≈ 2.71828)")
            .Returns(Number)
            .Template($"2.718281828459045");
       
       Function("Pi")
           .Doc("Возвращает математическую константу (π ≈ 3.14159)")
            .Returns(Number)
            .Template($"3.141592653589793");
    }
}
