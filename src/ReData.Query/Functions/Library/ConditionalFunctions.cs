using System.Runtime.InteropServices.JavaScript;
using ReData.Query.Core.Types;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;
using static DataType;

public class ConditionalFunctions : FunctionsDescriptor
{
    protected override void Functions()
    {
        int value = 0;
        foreach (var T in new[]
                 {
                     Number, Text, Integer, DateTime
                 })
        {
            Function("If")
                .Doc("Условное выражение: возвращает then-значение если condition=true, иначе else-значение")
                .Arg("condition", Bool, FunctionArgumentOptions.NotPropagateNull)
                .Arg("then", T)
                .Arg("else", T)
                .Returns(T)
                .Templates(new()
                {
                    [All] = $"CASE WHEN {0} THEN {1} ELSE {2} END",
                });

            Method("Alt")
                .Doc("Возвращает первое значение, если оно не NULL, иначе возвращает альтернативное значение")
                .Arg("input", T)
                .Arg("alt", T)
                .Returns(T)
                .CustomNullPropagation((nulls => nulls.All(x => x)))
                .Templates(new()
                {
                    [All] = $"COALESCE({0}, {1})",
                });
        }

        foreach (var T in new[]
                 {
                     Number, Text, Integer, DateTime, Unknown
                 })
        {
            Method("IsNull")
                .Doc("Проверяет, является ли значение NULL")
                .Arg("value", T)
                .ReturnsNotNull(Bool)
                .Templates(new()
                {
                    [All] = $"({value} IS NULL)",
                });

            Method("NotNull")
                .Doc("Проверяет, что значение не NULL")
                .Arg("value", T)
                .ReturnsNotNull(Bool)
                .Templates(new()
                {
                    [All] = $"({value} IS NOT NULL)",
                });

            Binary("=", T, Null)
                .Doc("Проверяет равенство")
                .ReturnsNotNull(Bool)
                .Templates(new()
                {
                    [All] = $"({value} IS NULL)",
                });

            Binary("!=", T, Null)
                .Doc("Проверяет неравенство")
                .ReturnsNotNull(Bool)
                .Templates(new()
                {
                    [All] = $"({value} IS NULL)",
                });
        }
    }
}