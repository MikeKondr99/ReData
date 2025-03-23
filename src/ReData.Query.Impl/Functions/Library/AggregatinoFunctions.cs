using System.Runtime.InteropServices.JavaScript;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;
using static DataType;

public class AggregationFunctions : FunctionsDescriptor
{
    protected override void Functions()
    {
        foreach (var T in new[] { Number, Integer, Text })
        {
            Function("ONCE")
                .Arg("value", T)
                .Returns(T)
                .Templates(new()
                {
                    [All] = $"(CASE WHEN COUNT(DISTINCT {0}) = 1 THEN MAX({0}) ELSE NULL END)"
                });
            
        }
        
        Function("COUNT")
            .Returns(Integer)
            .Templates(new()
            {
                [All] = $"COUNT()"
            });

        foreach (var T in new[] { Number, Integer, Text })
        {
            Function("COUNT")
                .Arg("value", T)
                .Returns(Integer)
                .Templates(new()
                {
                    [All] = $"COUNT(DISTINCT {0})"
                });
        }
        
        Function("COUNT")
            .Arg("value", Boolean)
            .Returns(Integer)
            .Templates(new()
            {
                [All] = $"COUNT(CASE WHEN {0} THEN 1 ELSE NULL)"
            });
        
        Function("SUM")
            .Arg("value", Number)
            .Returns(Number)
            .Templates(new()
            {
                [All] = $"SUM({0})"
            });
        
        // Sum c условием
        // SUM(price, id > 10)
        Function("SUM")
            .Arg("value", Number)
            .Arg("condition", Boolean)
            .Returns(Number)
            .Templates(new()
            {
                [All] = $"SUM(CASE WHEN {1} THEN {0} ELSE NULL)",
            });

    }
}