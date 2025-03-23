namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;
using static DataType;

public class LogicFunctions : FunctionsDescriptor
{
    protected override void Functions()
    {
        Binary("and", Boolean, Boolean)
            .Returns(Boolean)
            .Templates(new()
            {
                [All] = $"({0} AND {1})",
            });

        Binary("or", Boolean, Boolean)
            .Returns(Boolean)
            .Templates(new()
            {
                [All] = $"({0} OR {1})",
            });

        Function("Not")
            .Arg("input", Boolean)
            .Returns(Boolean)
            .Templates(new()
            {
                [All] = $"(NOT {0})",
            });
    }
}