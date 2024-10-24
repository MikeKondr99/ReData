using ReData.Query.Impl.QueryBuilders;

namespace ReData.Query.Impl.Tests.Functions.Conditional;

public abstract class Сommon(ISqlRunner runner) : ExprTests(runner)
{
     [Theory(DisplayName = "Если и логика")]
     [InlineData("If(true, 1,0)", 1)]
     [InlineData("If(false, 1,0)", 0)]
     [InlineData("If(Not(true), 1, 0)", 0)]
     [InlineData("If(Not(false), 1, 0)", 1)]
     [InlineData("If(true and true, 1, 0)", 1)]
     [InlineData("If(true and false, 1, 0)", 0)]
     [InlineData("If(false and false, 1, 0)", 0)]
     [InlineData("If(true or true, 1, 0)", 1)]
     [InlineData("If(true or false, 1, 0)", 1)]
     [InlineData("If(false or false, 1, 0)", 0)]
     public Task IfFunction(string expr, object? expected) => Test(expr, expected);
}