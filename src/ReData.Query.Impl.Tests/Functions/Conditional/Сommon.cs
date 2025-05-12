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
     [InlineData("If(true, null,0).Type()", "Int?")]
     [InlineData("If(true, null,0.0).Type()", "Num?")]
     [InlineData("If(true, null,'lol').Type()", "Text?")]
     
     [InlineData("If(null, 1, 0).Type()", "Int")]
     [InlineData("If(null, 1.0 ,0.0).Type()", "Num")]
     [InlineData("If(null, 'one','zero').Type()", "Text")]
     
     [InlineData("If(null, 'then', 'else')", "else")]
     [InlineData("If(10 > 5 and null, 'then', 'else')", "else")]
     [InlineData("If(true, 10, 15.5)", 10.0)]
     // [InlineData("If(true, false, true)", false)]
     public Task IfFunction(string expr, object? expected) => Test(expr, expected);
     
     
     [Theory(DisplayName = "Функция Or")]
     [InlineData("2.Alt(3).Type()", "Int")]
     [InlineData("Int(null).Alt(3).Type()", "Int")]
     [InlineData("2.Alt(Int(null)).Type()", "Int")]
     [InlineData("Int(null).Alt(Int(null)).Type()", "Int?")]
     [InlineData("1.Alt(2).Alt(3).Alt(4).Alt(5).Alt(6).Type()", "Int")]
     [InlineData("Int(null).Alt(null).Alt(null).Type()", "Int?")]
     
     [InlineData("2.Alt(3)", 2)]
     [InlineData("Int(null).Alt(3)", 3)]
     [InlineData("2.Alt(Int(null))", 2)]
     [InlineData("Int(null).Alt(Int(null))", null)]
     [InlineData("Int(null).Alt(If(true, 2, null))", 2)]
     [InlineData("1.Alt(2).Alt(3).Alt(4).Alt(5).Alt(6)", 1)]
     [InlineData("Int(null).Alt(null).Alt(If(true, 10, Int(null)))", 10)]
     // [InlineData("true.Or(false)", true)]
     public Task OrFunction(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "IsNull")]
     [InlineData("IsNull(null)", true)] // Direct null
     [InlineData("IsNull(42)", false)] // Number
     [InlineData("IsNull('text')", false)] // String
     [InlineData("IsNull('')", false)] // Empty string
     [InlineData("IsNull(0)", false)] // Zero value

     // Indirect null cases
     [InlineData("IsNull(1 + null)", true)] // Division by zero
     [InlineData("IsNull(Lower(Text(null)))", true)] // Division by zero
     // [InlineData("IsNull(5 = 5)", false)] // Boolean
     // [InlineData("IsNull(5 = null)", true)] // Boolean
     public Task FuncIsNullTests(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "NotNull")]
     [InlineData("NotNull(null)", false)] // Direct null
     [InlineData("NotNull(42)", true)] // Number
     [InlineData("NotNull('text')", true)] // String
     [InlineData("NotNull('')", true)] // Empty string
     [InlineData("NotNull(0)", true)] // Zero value

     // Indirect null cases
     [InlineData("NotNull(1 + null)", false)] // Division by zero
     [InlineData("NotNull(Lower(Text(null)))", false)] // Division by zero
     // [InlineData("NotNull(5 = 5)", true)] // Boolean
     // [InlineData("NotNull(5 = null)", false)] // Boolean
     public Task FuncNotNullTests(string expr, object? expected) => Test(expr, expected);
}