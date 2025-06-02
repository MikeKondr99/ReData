using ReData.Query.Core.Types;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypes;
using static DataType;

public class ComparisonFunctions : FunctionsDescriptor
{
    protected override void Functions()
    {
        foreach (var t in Types.AllWithoutBool)
        {
            Binary("=", t, t)
                .Doc("Проверяет равенство")
                .Returns(Bool)
                .Templates(new()
                {
                    [All] = $"({0} = {1})",
                });
            
            Binary("!=", t, t)
                .Doc("Проверяет неравенство")
                .Returns(Bool)
                .Templates(new()
                {
                    [All] = $"({0} <> {1})",
                });
        }
        
        
        foreach (var t in new [] { Number, Integer, DateTime })
        {
            Binary("<", t, t)
                .Doc("Проверяет, что первое значение строго меньше второго")
                .Returns(Bool)
                .Templates(new()
                {
                    [All] = $"({0} < {1})",
                });
            
            Binary(">", t, t)
                .Doc("Проверяет, что первое значение строго больше второго")
                .Returns(Bool)
                .Templates(new()
                {
                    [All] = $"({0} > {1})",
                });
            
            Binary("<=", t, t)
                .Doc("Проверяет, что первое значение меньше или равно второму")
                .Returns(Bool)
                .Templates(new()
                {
                    [All] = $"({0} <= {1})",
                });
            
            Binary(">=", t, t)
                .Doc("Проверяет, что первое значение больше или равно второму")
                .Returns(Bool)
                .Templates(new()
                {
                    [All] = $"({0} >= {1})",
                });

            Method("Between")
                .Doc("Проверяет, что значение находится в диапазоне [min, max] включительно")
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