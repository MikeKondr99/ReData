using ReData.Query.Impl.Runners;
using ReData.Query.Impl.Tests.Fixtures;

namespace ReData.Query.Impl.Tests.Functions.String;

public abstract class Сommon(IDatabaseFixture runner) : RawExprTests(runner)
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
     [InlineData("EmptyIsNull('').Type()", "Text?")]
     public Task FuncEmptyIsNullTests(string expr, object? expected) => Test(expr, expected);
     
     
     [Theory(DisplayName = "Len")]
     [InlineData("Len('')", 0)] // Empty string
     [InlineData("Len('a')", 1)] // Single char
     [InlineData("Len('hello')", 5)] // Basic string
     [InlineData("Len('привет')", 12)] // Unicode (6 cyrillic letters)
     [InlineData("Len('😀')", 4)] // Emoji (1 grapheme)
     [InlineData("Len('tab\t')", 4)] // Escape sequence
     [InlineData("Len(null)", null)] // Null case
     public Task StringLengthTests(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "Chr")]
     [InlineData("Chr(65)", "A")] // Uppercase A
     [InlineData("Chr(97)", "a")] // Lowercase a
     [InlineData("Chr(32)", " ")] // Space
     [InlineData("Chr(9)", "\t")] // Tab
     [InlineData("Chr(0)", "\0")] // Null char
     [InlineData("Chr(null)", null)] // Null input
     public Task FuncChrTests(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "Ord")]
     [InlineData("Ord('A')", 65)] // Uppercase A
     [InlineData("Ord('a')", 97)] // Lowercase a
     [InlineData("Ord(' ')", 32)] // Space
     [InlineData("Ord('\t')", 9)] // Tab
     [InlineData("Ord('')", 0)] // Empty string
     [InlineData("Ord('AB')", 65)] // First char only
     [InlineData("Ord('😀')", 240)] // Non-ASCII
     [InlineData("Ord(null)", null)] // Null input
     public Task FuncOrdTests(string expr, object? expected) => Test(expr, expected);
     
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
     
     
     [Theory(DisplayName = "Repeat")]
     // Normal cases
     [InlineData("Repeat('a', 3)", "aaa")] // Basic repetition
     [InlineData("'ab'.Repeat(2)", "abab")] // Multi-character strings
     [InlineData("Repeat(' ', 4)", "    ")] // Whitespace handling
     [InlineData("'😀'.Repeat(2)", "😀😀")] // Unicode/emoji support
     // Edge cases
     [InlineData("Repeat('text', 1)", "text")] // Single repetition (identity)
     [InlineData("'text'.Repeat(0)", "")] // Zero repetitions (empty string)
     [InlineData("Repeat('text', -1)", "")] // Negative repetitions (empty string)
     [InlineData("''.Repeat(5)", "")] // Empty input string
     // Null cases
     [InlineData("Repeat(null, 3)", null)] // Null input string
     [InlineData("Repeat('text', null)", null)] // Null count parameter
     public Task FuncRepeatTests(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "Composite")]
     [InlineData("'  HeLLo World! '.Trim().Lower()", "hello world!")]
     public Task Composite(string expr, object? expected) => Test(expr, expected);
     
}