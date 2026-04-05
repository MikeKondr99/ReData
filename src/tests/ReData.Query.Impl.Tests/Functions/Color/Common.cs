using ReData.Query.Impl.Tests.Fixtures;

namespace ReData.Query.Impl.Tests.Functions.Color;

public abstract class Common(IDatabaseFixture runner) : ExprTests(runner)
{
    [Theory(DisplayName = "Rgb")]
    [InlineData("Rgb(0,0,0)", 0xFF000000L)]
    [InlineData("Rgb(255,255,255)", 0xFFFFFFFFL)]
    [InlineData("Rgb(255,0,0)", 0xFFFF0000L)]
    [InlineData("Rgb(0,255,0)", 0xFF00FF00L)]
    [InlineData("Rgb(0,0,255)", 0xFF0000FFL)]
    [InlineData("Rgb(1,2,3)", 0xFF010203L)]
    [InlineData("Rgb(257,2,3)", 0xFF010203L)]
    [InlineData("Rgb(-1,0,0)", 0xFFFF0000L)]
    [InlineData("Rgb(0,-1,0)", 0xFF00FF00L)]
    [InlineData("Rgb(0,0,-1)", 0xFF0000FFL)]
    [InlineData("Rgb(null,0,0)", null)]
    [InlineData("Rgb(0,null,0)", null)]
    [InlineData("Rgb(0,0,null)", null)]
    [InlineData("Type(Rgb(1,2,3))", "int!")]
    public Task FuncRgbTests(string expr, object? expected) => Test(expr, expected);
}
