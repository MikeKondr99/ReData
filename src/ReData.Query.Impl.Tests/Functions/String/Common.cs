using ReData.Query.Impl.Runners;
using ReData.Query.Impl.Tests.Fixtures;

namespace ReData.Query.Impl.Tests.Functions.String;

public abstract class Сommon(IDatabaseFixture runner) : ExprTests(runner)
{
     
     [Theory(DisplayName = "Upper")]
     [InlineData("Upper('Hello World!')", "HELLO WORLD!")]
     public Task Upper(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "Lower")]
     [InlineData("Lower('Hello World!')", "hello world!")]
     public Task Lower(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "Trim")]
     [InlineData("Trim(' Hello ')", "Hello")]
     public Task Trim(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "TrimLeft")]
     [InlineData("TrimLeft(' Hello ')", "Hello ")]
     public Task TrimLeft(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "TrimRight")]
     [InlineData("TrimRight(' Hello ')", " Hello")]
     public Task TrimRight(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "Substring")]
     [InlineData("Substring('Hello World!',5)", "o World!")]
     [InlineData("Substring('Hello World!',5,3)", "o W")]
     public Task Substring(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "Reverse")]
     [InlineData("Reverse('Hello')", "olleH")]
     public Task Reverse(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "KeepChars", Skip = "Not implemented yet")]
     [InlineData("KeepChars('H2el1l3o','123')", "213")]
     public Task KeepChars(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "EmptyIsNull")]
     [InlineData("EmptyIsNull('Hello world!')", "Hello world!")]
     [InlineData("EmptyIsNull('')", null)]
     [InlineData("EmptyIsNull(null)", null)]
     public Task EmptyIsNull(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "Composite")]
     [InlineData("'  HeLLo World! '.Trim().Lower()", "hello world!")]
     public Task Composite(string expr, object? expected) => Test(expr, expected);
     
     [Theory]
}