using System.Runtime.InteropServices.JavaScript;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;
using static DataType;

public class ComparisonFunctions : FunctionsDescriptor
{
    protected override void Functions()
    {
        foreach (var T in new [] { Number, Integer, Bool, Text })
        {
            Binary("=", T, T)
                .Returns(Bool)
                .Templates(new()
                {
                    [All] = $"({0} = {1})",
                });
            
            Binary("!=", T, T)
                .Returns(Bool)
                .Templates(new()
                {
                    [All] = $"({0} <> {1})",
                });
        }
        
        foreach (var T in new [] { Number, Integer })
        {
            Binary("<", T, T)
                .Returns(Bool)
                .Templates(new()
                {
                    [All] = $"({0} < {1})",
                });
            
            Binary(">", T, T)
                .Returns(Bool)
                .Templates(new()
                {
                    [All] = $"({0} > {1})",
                });
            
            Binary("<=", T, T)
                .Returns(Bool)
                .Templates(new()
                {
                    [All] = $"({0} <= {1})",
                });
            
            Binary(">=", T, T)
                .Returns(Bool)
                .Templates(new()
                {
                    [All] = $"({0} >= {1})",
                });

            Method("Between")
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