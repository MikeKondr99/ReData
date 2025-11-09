using ReData.Query.Impl.Tests.Fixtures;

namespace ReData.Query.Impl.Tests.Literals;

public abstract class Сommon(IDatabaseFixture runner) : ExprTests(runner)
{
    [Theory(DisplayName = "Int")]
    [InlineData("21", 21)]
    [InlineData("00023", 23)]
    [InlineData("0", 0)]
    [InlineData("0000", 0)]
    [InlineData("2147483647", 2147483647)] // 32-bit max
    [InlineData("9223372036854775807", 9223372036854775807)] // 64-bit max (long)
    public Task Int(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "Number")]
    [InlineData("2.1", 2.1)]
    [InlineData("0.0", 0.0)]
    [InlineData(".0", 0.0)]
    [InlineData(".3", 0.3)]
    [InlineData("4.0", 4.0)]
    [InlineData("0.0001", 0.0001)]
    [InlineData("123.456", 123.456)]
    public Task Number(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "Text")]
    [InlineData("'Hello World!'", "Hello World!")]
    [InlineData("'contains $ inside'", "contains $ inside")]
    [InlineData("'line1\\nline2'", "line1\nline2")] // Escape sequences
    [InlineData("'tab\\tseparated'", "tab\tseparated")]
    [InlineData("'backslash\\\\here'", "backslash\\here")]
    [InlineData("'unicode: привет'", "unicode: привет")]
    [InlineData("'emoji: 😀👍'", "emoji: 😀👍")]
    [InlineData("''", "")] // Empty string
    [InlineData("'quoted: \\'text\\''", "quoted: 'text'")] // Escaped quotes if supported
    public Task Text(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "Bool")]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public Task Bool(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "Null")]
    [InlineData("'' + null", null)]
    public Task Null(string expr, object? expected) => Test(expr, expected);
}