namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;
using static DataType;

public class LogicFunctions : FunctionsDescriptor
{
    protected override void Functions()
    {
        Binary("and", Bool, Bool)
            .Returns(Bool)
            .Templates(new()
            {
                [All] = $"({0} AND {1})",
            });

        Binary("or", Bool, Bool)
            .Returns(Bool)
            .Templates(new()
            {
                [All] = $"({0} OR {1})",
            });

        Function("Not")
            .Arg("input", Bool)
            .Returns(Bool)
            .Templates(new()
            {
                [All] = $"(NOT {0})",
            });
    }
}