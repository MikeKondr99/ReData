using ReData.Query.Impl.QueryBuilders;

namespace ReData.Query.Impl.Tests.Functions.Conversion;

public abstract class Сommon(ISqlRunner runner) : ExprTests(runner)
{
     [Theory(DisplayName = "Int")]
     [InlineData("Int(25)", 25)]
     [InlineData("Int('25')", 25)]
     [InlineData("Int(2.6)", 2)]
     [InlineData("Int(false)", 0)]
     [InlineData("Int(true)", 1)]
     public Task Int(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "Num")]
     [InlineData("Num(25)", 25.0)]
     [InlineData("Num('25')", 25.0)]
     [InlineData("Num(2.5)", 2.5)]
     [InlineData("Num(false)", 0.0)]
     [InlineData("Num(true)", 1.0)]
     public Task Num(string expr, object? expected) => Test(expr, expected);
    
     [Theory(DisplayName = "Text")]
     [InlineData("Text(25)", "25")]
     [InlineData("Text(25000000)", "25000000")]
     [InlineData("Text('25')", "25")]
     [InlineData("Text(2.5)", "2.5")]
     [InlineData("Text(1000000.123)", "1000000.123")]
     [InlineData("Text(false)", "false")]
     [InlineData("Text(true)", "true")]
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
     public Task Bool(string expr, object? expected) => Test(expr, expected);
}