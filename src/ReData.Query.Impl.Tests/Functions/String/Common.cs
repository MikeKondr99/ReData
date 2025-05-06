using ReData.Query.Impl.Tests.Fixtures;

namespace ReData.Query.Impl.Tests.Functions.String;

public abstract class Сommon(IDatabaseFixture runner) : ExprExtensionTests(runner)
{
     
     [Theory(DisplayName = "Upper")]
     [InlineData("Upper('Hello World!')", "HELLO WORLD!")]
     public Task FuncUpperTetss(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "Lower")]
     [InlineData("Lower('Hello World!')", "hello world!")]
     public Task FuncLowerTests(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "Trim")]
     [InlineData("Trim(' Hello ')", "Hello")]
     public Task FuncTrimTests(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "TrimLeft")]
     [InlineData("TrimLeft(' Hello ')", "Hello ")]
     public Task FuncTrimLeftTests(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "TrimRight")]
     [InlineData("TrimRight(' Hello ')", " Hello")]
     public Task FuncTrimRightTests(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "Substring")]
     [InlineData("Substring('Hello World!',5)", "o World!")]
     [InlineData("Substring('Hello World!',5,3)", "o W")]
     public Task FuncSubstringTests(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "Reverse")]
     [InlineData("Reverse('Hello')", "olleH")]
     public Task FuncReverseTests(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "EmptyIsNull")]
     [InlineData("EmptyIsNull('Hello world!')", "Hello world!")]
     [InlineData("EmptyIsNull('')", null)]
     [InlineData("EmptyIsNull(null)", null)]
     public Task FuncEmptyIsNullTests(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "Replace")]
     // Basic replacements
     [InlineData("Replace('hello', 'l', 'x')", "hexxo")]
     [InlineData("Replace('hello', 'ell', 'ipp')", "hippo")]

     // Regex special characters (should be treated literally)
     [InlineData("Replace('a.*b', '.*', '-')", "a-b")] // Not regex wildcard
     [InlineData("Replace('a\\d+b', '\\d+', '-')", "a-b")] // Not regex digits
     [InlineData("Replace('a[b]', '[b]', 'c')", "ac")] // Not regex character class
     [InlineData("Replace('^$', '^', '!')", "!$")] // Not regex anchors

     // Edge cases
     [InlineData("Replace('', 'x', 'y')", "")]
     [InlineData("Replace('hello', 'x', 'y')", "hello")]
     [InlineData("Replace(null, 'a', 'b')", null)]
     [InlineData("Replace('hello', null, 'x')", null)]
     [InlineData("Replace('hello', 'l', null)", null)]
     public Task FuncReplaceTests(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "Composite")]
     [InlineData("'  HeLLo World! '.Trim().Lower()", "hello world!")]
     public Task Composite(string expr, object? expected) => Test(expr, expected);
     
}