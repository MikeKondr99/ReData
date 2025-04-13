using System.Runtime.InteropServices.JavaScript;

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
                .Arg("condition", Bool, propagateNull: false)
                .Arg("then", T)
                .Arg("else", T)
                .Returns(T)
                .Templates(new()
                {
                    [All] = $"CASE WHEN {0} THEN {1} ELSE {2} END",
                });

            Method("Or")
                .Arg("input", T)
                .Arg("alt", T)
                .Returns(T)
                .CustomNullPropagation((nulls => nulls.All(x => x)))
                .Templates(new()
                {
                    [All] = $"COALESCE({0}, {1})",
                });
            
            Method("IsNull")
                .Arg("value", T)
                .ReturnsNotNull(Bool)
                .Templates(new()
                {
                    [All] = $"({value} IS NULL)",
                });

            Method("NotNull")
                .Arg("value", T)
                .ReturnsNotNull(Bool)
                .Templates(new()
                {
                    [All] = $"({value} IS NOT NULL)",
                });
        }
    }
}
