using ReData.Query.Impl.QueryBuilders;
using ReData.Query.Impl.Tests.Fixtures;

namespace ReData.Query.Impl.Tests.Functions.Conversion;

public abstract class Сommon(IDatabaseFixture runner) : RawExprTests(runner)
{
     [Theory(DisplayName = "Int")]
     [InlineData("Int(25)", 25)]
     [InlineData("Int('25')", 25)]
     [InlineData("Int(2.6)", 2)]
     [InlineData("Int(false)", 0)]
     [InlineData("Int(true)", 1)]
     [InlineData("Int(null)", null)]
     public Task Int(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "Num")]
     [InlineData("Num(25)", 25.0)]
     [InlineData("Num('25')", 25.0)]
     [InlineData("Num(2.5)", 2.5)]
     [InlineData("Num(false)", 0.0)]
     [InlineData("Num(true)", 1.0)]
     [InlineData("Num(null)", null)]
     public Task Num(string expr, object? expected) => Test(expr, expected);
    
     [Theory(DisplayName = "Text")]
     [InlineData("Text(25)", "25")]
     [InlineData("Text(25000000)", "25000000")]
     [InlineData("Text('25')", "25")]
     [InlineData("Text(2.5)", "2.5")]
     [InlineData("Text(1000000.123)", "1000000.123")]
     [InlineData("Text(false)", "false")]
     [InlineData("Text(true)", "true")]
     [InlineData("Text(null)", null)]
     [InlineData("Date('2025-03-27 21:19').Text()", "2025-03-27 21:19:00")]
     [InlineData("Date('2025-03-27').Text()", "2025-03-27 00:00:00")]
     public Task Text(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "Bool")]
     [InlineData("Bool(25)", true)]
     [InlineData("Bool(0)", false)]
     [InlineData("Bool(-5)", false)]
     [InlineData("Bool(23)", true)]
     [InlineData("Bool(0.0)", false)]
     [InlineData("Bool(-5.0)", false)]
     [InlineData("Bool(23.0)", true)]
     [InlineData("Bool('25')", true)]
     [InlineData("Bool('')", false)]
     [InlineData("Bool(false)", false)]
     [InlineData("Bool(true)", true)]
     [InlineData("If(Bool(null), 'then', 'else')", "else")]
     public Task Bool(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "Date")]
     [InlineData("Date('2025-03-27 21:19')", "@2025-03-27 21:19Z")]
     [InlineData("'2025-03-27 21:40'.Date()", "@2025-03-27 21:40Z")]
     [InlineData("Date('2025-03-27')", "@2025-03-27 00:00Z")]
     public Task Date(string expr, object? expected) => Test(expr, expected);
}