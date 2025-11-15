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
    [InlineData("If(true, null,0).Type()", "int")]
    [InlineData("If(true, null,0.0).Type()", "num")]
    [InlineData("If(true, null,'lol').Type()", "text")]
    [InlineData("If(null, 1, 0).Type()", "int!")]
    [InlineData("If(null, 1.0 ,0.0).Type()", "num!")]
    [InlineData("If(null, 'one','zero').Type()", "text!")]
    [InlineData("If(null, 'then', 'else')", "else")]
    [InlineData("If(10 > 5 and null, 'then', 'else')", "else")]
    [InlineData("If(true, 10, 15.5)", 10.0)]

    // Тесты с датами
    [InlineData("If(true, Date(2023,1,1), Date(2023,12,31))", "@2023-01-01 00:00:00")]
    [InlineData("If(false, Date(2023,1,1), Date(2023,12,31))", "@2023-12-31 00:00:00")]
    [InlineData("If(null, Date(2023,1,1), Date(2023,12,31))", "@2023-12-31 00:00:00")]
    public Task IfFunction(string expr, object? expected) => Test(expr, expected);


    [Theory(DisplayName = "Функция Alt")]
    [InlineData("2.Alt(3).Type()", "int!")]
    [InlineData("Int(null).Alt(3).Type()", "int!")]
    [InlineData("2.Alt(Int(null)).Type()", "int!")]
    [InlineData("Int(null).Alt(Int(null)).Type()", "int")]
    [InlineData("1.Alt(2).Alt(3).Alt(4).Alt(5).Alt(6).Type()", "int!")]
    [InlineData("Int(null).Alt(null).Alt(null).Type()", "int")]
    [InlineData("2.Alt(3)", 2)]
    [InlineData("Int(null).Alt(3)", 3)]
    [InlineData("2.Alt(Int(null))", 2)]
    [InlineData("Int(null).Alt(Int(null))", null)]
    [InlineData("Int(null).Alt(If(true, 2, null))", 2)]
    [InlineData("1.Alt(2).Alt(3).Alt(4).Alt(5).Alt(6)", 1)]
    [InlineData("Int(null).Alt(null).Alt(If(true, 10, Int(null)))", 10)]
    // [InlineData("true.Or(false)", true)]
    [InlineData("Date(2023,1,1).Alt(Date(2023,12,31))", "@2023-01-01 00:00:00")]
    [InlineData("''.EmptyIsNull().Date().Alt(Date(2023,5,15))", "@2023-05-15 00:00:00")]
    [InlineData("3.14.Alt(2.71)", 3.14)]
    [InlineData("Num(null).Alt(1.618)", 1.618)]
    [InlineData("'first'.Alt('second')", "first")]
    [InlineData("Text(null).Alt('default')", "default")]
    public Task AltFunction(string expr, object? expected) => Test(expr, expected);

    [SkippableTheory(DisplayName = "IsNull")]
    [InlineData("IsNull(null)", true)] // Direct null
    [InlineData("IsNull(42)", false)] // Number
    [InlineData("IsNull('text')", false)] // String
    [InlineData("IsNull('')", false)] // Empty string
    [InlineData("IsNull(0)", false)] // Zero value

    // Indirect null cases
    [InlineData("IsNull(1 + null)", true)] // Division by zero
    [InlineData("IsNull(Lower(Text(null)))", true)] // Division by zero

    // Date
    [InlineData("IsNull(Date(2023,1,1))", false)]
    [InlineData("''.EmptyIsNull().Date().IsNull()", true)]

    // Num
    [InlineData("IsNull(3.14)", false)]
    [InlineData("IsNull(Num(null))", true)]
    public Task FuncIsNullTests(string expr, object? expected)
    {
        Skip.If(expr is "IsNull('')" && runner.GetDatabaseType() is DatabaseType.Oracle);
        return Test(expr, expected);
    }

    [SkippableTheory(DisplayName = "NotNull")]
    [InlineData("NotNull(null)", false)] // Direct null
    [InlineData("NotNull(42)", true)] // Number
    [InlineData("NotNull('text')", true)] // String
    [InlineData("NotNull('')", true)] // Empty string
    [InlineData("NotNull(0)", true)] // Zero value

    // Indirect null cases
    [InlineData("NotNull(1 + null)", false)] // Division by zero
    [InlineData("NotNull(Lower(Text(null)))", false)] // Division by zero

    // Date
    [InlineData("NotNull(Date(2023,1,1))", true)]
    [InlineData("''.EmptyIsNull().Date().NotNull()", false)]

    // Num
    [InlineData("NotNull(3.14)", true)]
    [InlineData("NotNull(Num(null))", false)]
    public Task FuncNotNullTests(string expr, object? expected)
    {
        Skip.If(expr is "NotNull('')" && runner.GetDatabaseType() is DatabaseType.Oracle);
        return Test(expr, expected);
    }
}