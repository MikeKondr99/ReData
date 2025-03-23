using ReData.Query.Impl.QueryBuilders;
using ReData.Query.Impl.Tests.Fixtures;

namespace ReData.Query.Impl.Tests.Functions.Conditional;

public abstract class Сommon(IDatabaseFixture runner) : ExprTests(runner)
{
     [Theory(DisplayName = "Базовая логика")]
     [InlineData("true", true)]
     [InlineData("false", false)]
     [InlineData("Not(true)", false)]
     [InlineData("Not(false)", true)]
     [InlineData("true and true", true)]
     [InlineData("true and false", false)]
     [InlineData("false and false", false)]
     [InlineData("true or true", true)]
     [InlineData("true or false", true)]
     [InlineData("false or false", false)]
     public Task BasicLogic(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "Функция If")]
     [InlineData("If(true, null,0).Type()", "Integer?")]
     [InlineData("If(true, null,0.0).Type()", "Number?")]
     [InlineData("If(true, null,'lol').Type()", "Text?")]
     [InlineData("If(true, null,false).Type()", "Boolean?")]
     
     [InlineData("If(null, 1, 0).Type()", "Integer")]
     [InlineData("If(null, 1.0 ,0.0).Type()", "Number")]
     [InlineData("If(null, 'one','zero').Type()", "Text")]
     [InlineData("If(null, true, false).Type()", "Boolean")]
     
     [InlineData("If(null, 'then', 'else')", "else")]
     [InlineData("If(10 > 5 and null, 'then', 'else')", "else")]
     [InlineData("If(true, 10, 15.5)", 10.0)]
     public Task IfFunction(string expr, object? expected) => Test(expr, expected);
     
     
     [Theory(DisplayName = "Функция Or")]
     [InlineData("2.Or(3).Type()", "Integer")]
     [InlineData("Int(null).Or(3).Type()", "Integer")]
     [InlineData("2.Or(Int(null)).Type()", "Integer")]
     [InlineData("Int(null).Or(Int(null)).Type()", "Integer?")]
     [InlineData("1.Or(2).Or(3).Or(4).Or(5).Or(6).Type()", "Integer")]
     [InlineData("Int(null).Or(null).Or(null).Type()", "Integer?")]
     
     [InlineData("2.Or(3)", 2)]
     [InlineData("Int(null).Or(3)", 3)]
     [InlineData("2.Or(Int(null))", 2)]
     [InlineData("Int(null).Or(Int(null))", null)]
     [InlineData("Int(null).Or(If(true, 2, null))", 2)]
     [InlineData("1.Or(2).Or(3).Or(4).Or(5).Or(6)", 1)]
     [InlineData("Int(null).Or(null).Or(If(true, 10, Int(null)))", 10)]
     public Task OrFunction(string expr, object? expected) => Test(expr, expected);
}