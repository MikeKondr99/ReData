using ReData.Query.Core.Types;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;
using static DataType;

public class ComparisonFunctions : FunctionsDescriptor
{
    protected override void Functions()
    {
        foreach (var T in Types.AllWithoutBool)
        {
            Binary("=", T, T)
                .Doc("Проверяет равенство")
                .Returns(Bool)
                .Templates(new()
                {
                    [All] = $"({0} = {1})",
                });
            
            Binary("!=", T, T)
                .Doc("Проверяет неравенство")
                .Returns(Bool)
                .Templates(new()
                {
                    [All] = $"({0} <> {1})",
                });
        }
        
        
        foreach (var T in new [] { Number, Integer, DateTime })
        {
            Binary("<", T, T)
                .Doc("Проверяет, что первое значение строго меньше второго")
                .Returns(Bool)
                .Templates(new()
                {
                    [All] = $"({0} < {1})",
                });
            
            Binary(">", T, T)
                .Doc("Проверяет, что первое значение строго больше второго")
                .Returns(Bool)
                .Templates(new()
                {
                    [All] = $"({0} > {1})",
                });
            
            Binary("<=", T, T)
                .Doc("Проверяет, что первое значение меньше или равно второму")
                .Returns(Bool)
                .Templates(new()
                {
                    [All] = $"({0} <= {1})",
                });
            
            Binary(">=", T, T)
                .Doc("Проверяет, что первое значение больше или равно второму")
                .Returns(Bool)
                .Templates(new()
                {
                    [All] = $"({0} >= {1})",
                });

            Method("Between")
                .Doc("Проверяет, что значение находится в диапазоне [min, max] включительно")
                .Arg("input", T)
                .Arg("min", T)
                .Arg("max", T)
                .Returns(Bool)
                .Templates(new()
                {
                    [All] = $"({0} BETWEEN {1} AND {2})",
                });
        }
    }
}