using System.Runtime.InteropServices.JavaScript;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;
using static DataType;

public class ConditionalFunctions : FunctionsDescriptor
{
    protected override void Functions()
    {
        foreach (var T in new [] { Number, Text, Boolean, Integer })
        {
            Function("If")
                .Arg("condition", Boolean, propagateNull: false)
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
        }
    }
}