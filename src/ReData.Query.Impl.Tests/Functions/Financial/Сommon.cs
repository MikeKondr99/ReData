using ReData.Query.Impl.QueryBuilders;
using ReData.Query.Impl.Tests.Fixtures;

namespace ReData.Query.Impl.Tests.Functions.Financial;

public abstract class Сommon(IDatabaseFixture runner) : RawExprTests(runner)
{
     [Theory(DisplayName = "Финансовые функции")]
     // Google Sheets 786.7220993
     [InlineData("FutureValue(0.005, 36,-20.0).Text().Substring(1,9)", "786.72209")]
     [InlineData("FutureValue(null, 36,-20.0)", null)]
     [InlineData("FutureValue(0.005, null,-20.0)", null)]
     [InlineData("FutureValue(0.005, 36, null)", null)]
     public Task FinancialFunctions(string expr, object? expected) => Test(expr, expected);
}