using ReData.Query.Core.Template;
using ReData.Query.Core.Types;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;
using static DataType;

public class MathFunctions : FunctionsDescriptor
{
    protected override void Functions()
    {
        foreach (var T in new [] { Integer, Number })
        {
            Binary("+", T, T)
                .Doc("Сложение двух чисел")
                .Returns(T)
                .Templates(new()
                {
                    [All] = $"({0} + {1})",
                });
            
            Binary("-", T, T)
                .Doc("Вычитание двух чисел")
                .Returns(T)
                .Templates(new()
                {
                    [All] = $"({0} - {1})",
                });

            Unary("-")
                .Doc("Унарный минус числа")
                .Arg("value", T)
                .Returns(T)
                .Templates(new()
                {
                    [All] = $"(-{0})",
                });
            
            Binary("*", T, T)
                .Doc("Умножение двух чисел")
                .Returns(T)
                .Templates(new()
                {
                    [All] = $"({0} * {1})", 
                });
        }

        Binary("/", Number, Number)
            .Doc("Деление чисел с плавающей точкой")
            .Returns(Number)
            .Templates(new()
            {
                [All] = $"({0} / {1})", 
            });
        
        Binary("/", Integer, Integer)
            .Doc("Целочисленное деление (с отбрасыванием остатка)")
            .Returns(Integer)
            .Templates(new()
            {
                [PostgreSql | SqlServer] = $"({0} / {1})", 
                [MySql] = $"({0} DIV {1})", 
                [Oracle] = $"FLOOR({0} / {1})", 
                [ClickHouse] = $"intDiv({0}, {1})" 
            });

        Dictionary<DatabaseTypeFlags, TemplateInterpolatedStringHandler> powTemplates = new()
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
           .Doc("Возвращает математическую константу e (≈2.71828)")
            .Returns(Number)
            .Template($"2.718281828459045");
       
       Function("Pi")
           .Doc("Возвращает математическую константу π (≈3.14159)")
            .Returns(Number)
            .Template($"3.141592653589793");
    }
}