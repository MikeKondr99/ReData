using System.Runtime.InteropServices.JavaScript;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;
using static BinaryOperation;
using static DataType;

public class ComparisonFunctions : FunctionsDescriptor
{
    protected override void Functions()
    {
        foreach (var T in new [] { Number, Integer, Boolean, Text })
        {
            Binary("=", T, T)
                .Returns(Boolean)
                .Templates(new()
                {
                    [All] = $"({0} = {1})",
                });
            
            Binary("!=", T, T)
                .Returns(Boolean)
                .Templates(new()
                {
                    [All] = $"({0} <> {1})",
                });
        }
        
        foreach (var T in new [] { Number, Integer, Text })
        {
            Method("IsNull")
                .Arg("value", T)
                .ReturnsNotNull(Boolean)
                .Templates(new()
                {
                    [All] = $"({0} IS NULL)",
                });
        }
        
        foreach (var T in new [] { Number, Integer })
        {
            Binary("<", T, T)
                .Returns(Boolean)
                .Templates(new()
                {
                    [All] = $"({0} < {1})",
                });
            
            Binary(">", T, T)
                .Returns(Boolean)
                .Templates(new()
                {
                    [All] = $"({0} > {1})",
                });
            
            Binary("<=", T, T)
                .Returns(Boolean)
                .Templates(new()
                {
                    [All] = $"({0} <= {1})",
                });
            
            Binary(">=", T, T)
                .Returns(Boolean)
                .Templates(new()
                {
                    [All] = $"({0} >= {1})",
                });

            Method("Between")
                .Arg("input", T)
                .Arg("min", T)
                .Arg("max", T)
                .Returns(Boolean)
                .Templates(new()
                {
                    [All] = $"({0} BETWEEN {1} AND {2})",
                });
        }
    }
}