using ReData.Query.Core.Types;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypes;
using static DataType;

public class ComparisonFunctions : FunctionsDescriptor
{
    public static string TypeNames2(DataType dataType) => dataType switch
    {
        Integer => "целых чисел",
        Number => "дробных чисел",
        Text => "текстов",
        DateTime => "дат",
        
        

    };
    protected override void Functions()
    {
        foreach (var t in Types.AllWithoutBool)
        {
            Binary("=", t, t)
                .Doc($"Проверяет равенство двух {TypeNames2(t)}")
                .Returns(Bool)
                .Templates(new()
                {
                    [All] = $"({0} = {1})",
                });
            
            Binary("!=", t, t)
                .Doc($"Проверяет неравенство двух {TypeNames2(t)}")
                .Returns(Bool)
                .Templates(new()
                {
                    [All] = $"({0} <> {1})",
                });
        }
        
        
        foreach (var t in new [] { Number, Integer, DateTime })
        {
            Binary("<", t, t)
                .Doc($"Проверяет, что первое значение строго меньше второго для двух {TypeNames2(t)}")
                .Returns(Bool)
                .Templates(new()
                {
                    [All] = $"({0} < {1})",
                });
            
            Binary(">", t, t)
                .Doc($"Проверяет, что первое значение строго больше второго для двух {TypeNames2(t)}")
                .Returns(Bool)
                .Templates(new()
                {
                    [All] = $"({0} > {1})",
                });
            
            Binary("<=", t, t)
                .Doc($"Проверяет, что первое значение меньше или равно второму для двух {TypeNames2(t)}")
                .Returns(Bool)
                .Templates(new()
                {
                    [All] = $"({0} <= {1})",
                });
            
            Binary(">=", t, t)
                .Doc($"Проверяет, что первое значение больше или равно второму для двух {TypeNames2(t)}")
                .Returns(Bool)
                .Templates(new()
                {
                    [All] = $"({0} >= {1})",
                });

            Method("Between")
                .Doc($"Проверяет, что значение находится в диапазоне [min, max] включительно для {TypeNames2(t)}")
                .Arg("input", t)
                .Arg("min", t)
                .Arg("max", t)
                .Returns(Bool)
                .Templates(new()
                {
                    [All] = $"({0} BETWEEN {1} AND {2})",
                });
        }
    }
}