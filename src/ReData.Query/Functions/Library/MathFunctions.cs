using ReData.Query.Core.Template;
using ReData.Query.Core.Types;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypes;
using static DataType;

public class MathFunctions : FunctionsDescriptor
{
    protected override void Functions()
    {
        foreach (var type in new [] { Integer, Number })
        {
            Binary("+", type, type)
                .Doc("Сложение двух чисел")
                .Returns(type)
                .Templates(new()
                {
                    [All] = $"({0} + {1})",
                });
            
            Binary("-", type, type)
                .Doc("Вычитание двух чисел")
                .Returns(type)
                .Templates(new()
                {
                    [All] = $"({0} - {1})",
                });

            Unary("-")
                .Doc("Унарный минус числа")
                .Arg("value", type)
                .Returns(type)
                .Templates(new()
                {
                    [All] = $"(-{0})",
                });
            
            Binary("*", type, type)
                .Doc("Умножение двух чисел")
                .Returns(type)
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
           .Doc("Возвращает математическую константу e (≈2.71828)")
            .Returns(Number)
            .Template($"2.718281828459045");
       
       Function("Pi")
           .Doc("Возвращает математическую константу π (≈3.14159)")
            .Returns(Number)
            .Template($"3.141592653589793");
    }
}