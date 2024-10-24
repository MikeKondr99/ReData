namespace ReData.Query.Impl.Functions.Library;

using static DatabaseTypeFlags;

public static class FinancialFunctions
{
    public static Ret<Number> FutureValue(Number rate, Number nper, Number pmt) => new()
    {
        [All & ~SqlServer] = $"(-1 * {pmt} * (1 - POWER(1 + {rate}, -{nper})) / {rate}) * POWER(1 + {rate}, {nper})", 
        [SqlServer] = $"(-1.0 * CAST({pmt} AS DECIMAL(30,20)) * (1.0 - POWER(1 + CAST({rate} AS DECIMAL(30,20)), -{nper})) / {rate}) * POWER(1 + CAST({rate} AS DECIMAL(30,20)), {nper})", 
    };
    
}