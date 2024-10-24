using ReData.Query.Impl.QueryBuilders;

namespace ReData.Query.Impl.Tests.Functions.Comparison;

public abstract class Сommon(ISqlRunner runner) : ExprTests(runner)
{
     // [Theory(DisplayName = "Финансовые функции")]
     // [InlineData("Substring(Text(FutureValue(0.005, 36,-20.0)), 1, 7)", "786.722")]
     public Task Less(string expr, object? expected) => Test(expr, expected);
}