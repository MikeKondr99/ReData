using ReData.Query.Core.Types;

namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;
using static DataType;

public class FinancialFunctions : FunctionsDescriptor
{
    protected override void Functions()
    {
        int rate = 0, nper = 1, pmt = 2;
        Function("FutureValue")
            .Doc("Вычисляет будущую стоимость инвестиций на основе постоянной процентной ставки, количества периодов и постоянных платежей")
            .Arg("rate", Number)
            .Arg("nper", Integer)
            .Arg("pmt", Number)
            .Returns(Number)
            .Templates(new()
            {
                [All & ~SqlServer] =
                    $"(-1 * {pmt} * (1 - POWER(1 + {rate}, -{nper})) / {rate}) * POWER(1 + {rate}, {nper})",
                [SqlServer] =
                    $"(-1.0 * CAST({pmt} AS DECIMAL(30,20)) * (1.0 - POWER(1 + CAST({rate} AS DECIMAL(30,20)), -{nper})) / {rate}) * POWER(1 + CAST({rate} AS DECIMAL(30,20)), {nper})",
            });
    }
}