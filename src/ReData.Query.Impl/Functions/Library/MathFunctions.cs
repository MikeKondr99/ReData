using ReData.Query.Visitors;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;
using static DataType;

public class MathFunctions : FunctionsDescriptor
{
    protected override void Functions()
    {
        foreach (var T in new [] { Integer, Number })
        {
            Binary("+", T, T)
                .Returns(T)
                .Templates(new()
                {
                    [All] = $"({0} + {1})",
                });
            
            Binary("-", T, T)
                .Returns(T)
                .Templates(new()
                {
                    [All] = $"({0} - {1})",
                });

            Unary("-")
                .Arg("value", T)
                .Returns(T)
                .Templates(new()
                {
                    [All] = $"(-{0})",
                });
            
            Binary("*", T, T)
                .Returns(T)
                .Templates(new()
                {
                    [All] = $"({0} * {1})", 
                });
        }

        Binary("/", Number, Number)
            .Returns(Number)
            .Templates(new()
            {
                [All] = $"({0} / {1})", 
            });
        
        Binary("/", Integer, Integer)
            .Returns(Integer)
            .Templates(new()
            {
                [PostgreSql | SqlServer] = $"({0} / {1})", 
                [MySql] = $"({0} DIV {1})", 
                [Oracle] = $"FLOOR({0} / {1})", 
                [ClickHouse] = $"intDiv({0}, {1})" 
            });

        Dictionary<DatabaseTypeFlags, TemplateInterpolatedStringHandler> powTemplates = new()
        {
            [All] = $"POWER({0}, {1})",
        };

        Method("Pow")
            .Arg("left", Number)
            .Arg("right", Number)
            .Returns(Number)
            .Templates(powTemplates);
        
       Binary("^", Number, Number)
            .Returns(Number)
            .Templates(powTemplates);
       
       // TODO: Not Tested
       Function("E")
            .Returns(Number)
            .Template($"2.718281828459045");
       
       // TODO: Not Tested
       Function("Pi")
            .Returns(Number)
            .Template($"3.141592653589793");
    }
}