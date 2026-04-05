using ReData.Query.Core.Types;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypes;
using static DataType;

public class ColorFunctions : FunctionsDescriptor
{

    protected override void Functions()
    {
        Function("Rgb")
            .Doc("Создает цвет из компонентов RGB (0-255) в формате 0xFFRRGGBB")
            .Arg("r", Integer)
            .Arg("g", Integer)
            .Arg("b", Integer)
            .Returns(Integer)
            .TemplatesX((r, g, b) => new()
            {
                [SqlServer] = $"(CAST(0xFF000000 AS BIGINT) | ((CAST({r} AS BIGINT) & 255) << 16) | ((CAST({g} AS BIGINT) & 255) << 8) | (CAST({b} AS BIGINT) & 255))",
                [MySql] = $"CAST((0xFF000000 | (({r} & 255) << 16) | (({g} & 255) << 8) | ({b} & 255)) AS UNSIGNED)",
                [PostgreSql] = $"(4278190080::BIGINT | (({r}::BIGINT & 255) << 16) | (({g}::BIGINT & 255) << 8) | ({b}::BIGINT & 255))",
                [Oracle] = $"TO_NUMBER('FF000000', 'XXXXXXXX') + BITAND({r}, 255) * 65536 + BITAND({g}, 255) * 256 + BITAND({b}, 255)",
                [ClickHouse] = $"bitOr(toUInt64(0xFF000000), bitOr(bitShiftLeft(bitAnd(toUInt64({r}), 255), 16), bitOr(bitShiftLeft(bitAnd(toUInt64({g}), 255), 8), bitAnd(toUInt64({b}), 255))))"
            });
    }
}
