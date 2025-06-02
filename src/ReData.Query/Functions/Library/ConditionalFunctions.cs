using System.Runtime.InteropServices.JavaScript;
using ReData.Query.Core.Types;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypes;
using static DataType;

public class ConditionalFunctions : FunctionsDescriptor
{
    protected override void Functions()
    {
        int value = 0;
        foreach (var type in new[]
                 {
                     Number, Text, Integer, DateTime
                 })
        {
            Function("If")
                .Doc("Условное выражение: возвращает then-значение если condition=true, иначе else-значение")
                .Arg("condition", Bool, propagateNull: false)
                .Arg("then", type)
                .Arg("else", type)
                .Returns(type)
                .Templates(new()
                {
                    [All] = $"CASE WHEN {0} THEN {1} ELSE {2} END",
                });

            Method("Alt")
                .Doc("Возвращает первое значение, если оно не NULL, иначе возвращает альтернативное значение")
                .Arg("input", type)
                .Arg("alt", type)
                .Returns(type)
                .CustomNullPropagation(nulls => nulls.All(x => x))
                .Templates(new()
                {
                    [All] = $"COALESCE({0}, {1})",
                });
        }

        foreach (var type in new[]
                 {
                     Number, Text, Integer, DateTime, Unknown
                 })
        {
            Method("IsNull")
                .Doc("Проверяет, является ли значение NULL")
                .Arg("value", type)
                .ReturnsNotNull(Bool)
                .Templates(new()
                {
                    [All] = $"({value} IS NULL)",
                });

            Method("NotNull")
                .Doc("Проверяет, что значение не NULL")
                .Arg("value", type)
                .ReturnsNotNull(Bool)
                .Templates(new()
                {
                    [All] = $"({value} IS NOT NULL)",
                });

            Binary("=", type, Null)
                .Doc("Проверяет равенство")
                .ReturnsNotNull(Bool)
                .Templates(new()
                {
                    [All] = $"({value} IS NULL)",
                });

            Binary("!=", type, Null)
                .Doc("Проверяет неравенство")
                .ReturnsNotNull(Bool)
                .Templates(new()
                {
                    [All] = $"({value} IS NULL)",
                });
        }
    }
}