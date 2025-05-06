using ReData.Query.Core.Template;
using ReData.Query.Core.Types;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;
using static DataType;

public class ImplicitConversionFunctions : FunctionsDescriptor
{
    private FunctionBuilder Convertion(DataType input, DataType ret, int level)
    {
        var name = ret switch
        {
            Unknown => "Unknown",
            Null => "Null",
            Number => "Num",
            Integer => "Int",
            DataType.Text => "Text",
            Bool => "Bool",
        };
        return Function(name)
            .Arg("input", input)
            .Returns(ret);
    }

    protected override void Functions()
    {
        // Здесь возможно важен порядок
        Function("IntegerFromNull")
            .ImplicitCast(1)
            .Arg("input", Null)
            .Returns(Integer)
            .Template($"SIGN({0})");
        
        
        Function("NumberFromNull")
            .ImplicitCast(2)
            .Arg("input", Null)
            .Returns(Number)
            .Template($"SIGN({0})");


        Function("BoolFromNull")
            .ImplicitCast(1)
            .Arg("input", Null)
            .Returns(Bool)
            .Template($"({0} = 0)");

        Function("TextFromNull")
            .ImplicitCast(1)
            .Arg("input", Null)
            .Returns(Text)
            .Template($"LOWER({0})");

        foreach (var T in new[]
                 {
                     Integer, Number, Bool, Text, DateTime
                 })
        {
            Function("Optional")
                .ImplicitCast(1)
                .ReqArg("input", T)
                .Returns(T)
                .CustomNullPropagation(nulls => true)
                .Template($"{0}");
        }

        Dictionary<DatabaseTypeFlags, TemplateInterpolatedStringHandler> IntegerToNumbers = new()
        {
            [All & ~ClickHouse & ~ Oracle] = $"CAST({0} AS DECIMAL(30,15))",
            [Oracle] = $"CAST({0} AS NUMERIC)",
            [ClickHouse] = $"toFloat64({0})"
        };

        Function("ToNum")
            .ImplicitCast(3)
            .ReqArg("input", Integer)
            .ReturnsNotNull(Number)
            .Templates(IntegerToNumbers);

        Function("ToNum")
            .ImplicitCast(3)
            .Arg("input", Integer)
            .Returns(Number)
            .Templates(IntegerToNumbers);

        Function("ToNum2")
            .ImplicitCast(3)
            .ReqArg("input", Integer)
            .Returns(Number)
            .Templates(IntegerToNumbers);
    }
}