using ReData.Query.Impl.QueryBuilders;

namespace ReData.Query.Impl.Tests.Functions.Financial;

public abstract class Сommon(ISqlRunner runner) : ExprTests(runner)
{
     [Theory(DisplayName = "Финансовые функции")]
     // Google Sheets 786.7220993
     [InlineData("Substring(Text(FutureValue(0.005, 36,-20.0)),1,9)", "786.72209")]
     [InlineData("FutureValue(null, 36,-20.0)", null)]
     [InlineData("FutureValue(0.005, null,-20.0)", null)]
     [InlineData("FutureValue(0.005, 36, null)", null)]
     public Task FinancialFunctions(string expr, object? expected) => Test(expr, expected);
}