using ReData.Query.Impl.QueryBuilders;

namespace ReData.Query.Impl.Tests.Functions.Number;

public abstract class Сommon(ISqlRunner runner) : ExprTests(runner)
{
     [Theory(DisplayName = "Floor")]
     [InlineData("Floor(1.2)", 1)]
     [InlineData("Floor(1.8)", 1)]
     [InlineData("Floor(4.7, 2.0)", 4.0)]
     [InlineData("Floor(2.4)", 2)]
     [InlineData("Floor(4.2)", 4)]
     [InlineData("Floor(3.88, .1)", 3.8)]
     [InlineData("Floor(3.88, 5.0)", 0)]
     [InlineData("Floor(1.1, 1.0)", 1)]
     [InlineData("Floor(4.7, .5)", 4.5)]
     [InlineData("Floor(1.1, 1.0, 0.5)", 0.5)]
     [InlineData("Floor(-150.0, 50.0, 25.0)", -175)]
     public Task Floor(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "Ceil")]
     [InlineData("Ceil(1.2)", 2)]
     [InlineData("Ceil(1.8)", 2)]
     [InlineData("Ceil(4.7, .5)", 5.0)]
     [InlineData("Ceil(4.7, 2.0)", 6.0)]
     [InlineData("Ceil(1.1, 1.0, -0.01)", 1.99)]
     public Task Ceil(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "Round")]
     [InlineData("Round(1.2)", 1.0)]
     [InlineData("Round(1.8)", 2.0)]
     [InlineData("Round(0.5)", 1.0)]
     [InlineData("Round(0.7)", 1.0)]
     [InlineData("Round(4.7, 2.0)", 4.0)]
     [InlineData("Round(5.3, 2.0)", 6.0)]
     [InlineData("Round(4.7, .5)", 4.5)]
     [InlineData("Round(5.3, .5)", 5.5)]
     [InlineData("Round(2.5, 1.0)", 3)]
     [InlineData("Round(2.0, 4.0)", 4)]
     [InlineData("Round(3.88875, 0.001)", 3.8890000000000000001)]
     [InlineData("Round(1.1, 1.0, .5)",1.5)]
     [InlineData("Round(2.5, 1.0, .0)", 3.0)] // не проходит
     [InlineData("Round(2.0, 4.0, .0)", 4.0)] 
     [InlineData("Round(100.0, 1.0, 200.0)", 100)]
     [InlineData("Round(-130.0, 50.0, 25.0)", -125)]
     public Task Round(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "Mod")]
     [InlineData("Mod(10, 5)", 0)]
     [InlineData("Mod(6, 5)", 1)]
     [InlineData("Mod(-9, 5)", 4)]
     public Task Mod(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "Abs")]
     [InlineData("Abs(1.5)", 1.5)]
     [InlineData("Abs(.0 -1.5)", 1.5)]
     [InlineData("Abs(3)", 3)]
     [InlineData("Abs(-3)", 3)]
     public Task Abs(string expr, object? expected) => Test(expr, expected);
     
}