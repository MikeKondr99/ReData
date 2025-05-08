using ReData.Query.Core.Types;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;
using static DataType;

public class LogicFunctions : FunctionsDescriptor
{
    protected override void Functions()
    {
        Binary("and", Bool, Bool)
            .Doc("Логическое И (возвращает true только если оба операнда true)")
            .Returns(Bool)
            .Templates(new()
            {
                [All] = $"({0} AND {1})",
            });

        Binary("or", Bool, Bool)
            .Doc("Логическое ИЛИ (возвращает true если хотя бы один операнд true)")
            .Returns(Bool)
            .Templates(new()
            {
                [All] = $"({0} OR {1})",
            });

        Method("Not")
            .Doc("Логическое отрицание (возвращает true если входное значение false, и наоборот)")
            .Arg("input", Bool)
            .Returns(Bool)
            .Templates(new()
            {
                [All] = $"(NOT {0})",
            });
    }
}