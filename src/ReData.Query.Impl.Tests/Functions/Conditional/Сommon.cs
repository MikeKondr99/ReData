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

    [Theory(DisplayName = "Функция Xor")]
    [InlineData("Xor(true, true)", false)]
    [InlineData("Xor(true, false)", true)]
    [InlineData("Xor(false, true)", true)]
    [InlineData("Xor(false, false)", false)]
    [InlineData("Xor(null, true)", null)]
    [InlineData("Xor(true, null)", null)]
    [InlineData("Xor(null, false)", null)]
    [InlineData("Xor(false, null)", null)]
    [InlineData("Xor(null, null)", null)]
    [InlineData("Type(Xor(true, false))", "bool!")]
    [InlineData("Type(Xor(Bool(null), true))", "bool")]
    public Task XorFunction(string expr, object? expected) => Test(expr, expected);

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
    // Тест на short circuit
    [InlineData("If(true, 0, 12/0)", 0)]
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
    
    [Theory(DisplayName = "Case(condition, value)")]
    [InlineData("Case(true, 'text')", "text")]
    [InlineData("Case(false, 'text')", null)]
    [InlineData("Case(null, 'text')", null)]
    [InlineData("Case(true, 42)", 42)]
    [InlineData("Case(false, 42)", null)]
    [InlineData("Case(true, 3.14)", 3.14)]
    [InlineData("Case(false, 3.14)", null)]
    [InlineData("Case(true, Date(2023,1,1))", "@2023-01-01 00:00:00")]
    [InlineData("Case(false, Date(2023,1,1))", null)]
    [InlineData("Type(Case(true, 'text'))", "text")]
    [InlineData("Type(Case(false, 'text'))", "text")]
    [InlineData("Type(Case(true, 42))", "int")]
    [InlineData("Type(Case(false, 42))", "int")]
    public Task FuncCaseConditionTests(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "Case(input, condition, value)")]
    [InlineData("Case('input', true, 'other')", "input")]
    [InlineData("Case('input', false, 'other')", "input")]
    [InlineData("Case(null, true, 'other')", "other")]
    [InlineData("Case(null, false, 'other')", null)]
    [InlineData("Case(42, true, 100)", 42)]
    [InlineData("Case(42, false, 100)", 42)]
    [InlineData("Case(null, true, 100)", 100)]
    [InlineData("Case(null, false, 100)", null)]
    [InlineData("Case(3.14, true, 2.71)", 3.14)]
    [InlineData("Case(null, true, 2.71)", 2.71)]
    [InlineData("Case(Date(2023,1,1), true, Date(2023,12,31))", "@2023-01-01 00:00:00")]
    [InlineData("Type(Case('input', true, 'other'))", "text")]
    [InlineData("Type(Case(null, true, 'other'))", "text")]
    [InlineData("Type(Case(null, false, 'other'))", "text")]
    [InlineData("Type(Case(42, true, 100))", "int")]
    public Task FuncCaseInputTests(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "Chained Case with Alt")]
    [InlineData("Case(true, 'young').Case(true, 'adult').Alt('old')", "young")]
    [InlineData("Case(false, 'young').Case(true, 'adult').Alt('old')", "adult")]
    [InlineData("Case(false, 'young').Case(false, 'adult').Alt('old')", "old")]
    [InlineData("Case(null, 'young').Case(true, 'adult').Alt('old')", "adult")]
    [InlineData("Type(Case(false, 1).Case(false, 2).Alt(3))", "int!")]
    [InlineData("Type(Case(false, 1).Case(false, 2).Alt(null))", "int")]
    [InlineData("Type(Case(true, 'text').Case(false, 'other').Alt('default'))", "text!")]
    public Task FuncChainedCaseTests(string expr, object? expected) => Test(expr, expected);
    
    
}
