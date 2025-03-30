using ReData.Query.Impl.QueryBuilders;
using ReData.Query.Impl.Runners;
using ReData.Query.Impl.Tests.Fixtures;

namespace ReData.Query.Impl.Tests.Literals;

public abstract class Сommon(IDatabaseFixture runner) : RawExprTests(runner)
{
     
    [Theory(DisplayName = "Int")]
    [InlineData("21", 21)]
    [InlineData("00023", 23)]
    public Task Int(string expr, object? expected) => Test(expr, expected);
     
    [Theory(DisplayName = "Number")]
    [InlineData("2.1", 2.1)]
    [InlineData("0.0", 0.0)]
    [InlineData(".0", 0.0)]
    [InlineData(".3", 0.3)]
    [InlineData("4.0", 4.0)]
    public Task Number(string expr, object? expected) => Test(expr, expected);
     
    [Theory(DisplayName = "Text")]
    [InlineData("'Hello World!'", "Hello World!")]
    // [InlineData(@"'Hello\'World!'", "Hello'World!")]
    public Task Text(string expr, object? expected) => Test(expr, expected);
     
    [Theory(DisplayName = "Bool")]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public Task Bool(string expr, object? expected) => Test(expr, expected);

}