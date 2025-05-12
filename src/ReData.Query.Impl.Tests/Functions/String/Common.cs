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
     
     [Theory(DisplayName = "Find")]
     [InlineData("Find('abc', 'a')", 1)]       // Первый символ
     [InlineData("Find('abc', 'b')", 2)]       // Середина строки
     [InlineData("Find('abc', 'c')", 3)]       // Последний символ
     [InlineData("Find('abc', 'bc')", 2)]      // Подстрока
     [InlineData("Find('abc', 'd')", null)]    // Не найдено
     [InlineData("Find('', 'a')", null)]       // Пустая строка
     [InlineData("Find('abc', '')", 1)]        // Поиск пустой подстроки
     [InlineData("Find('aabaa', 'aa')", 1)]    // Первое вхождение
     [InlineData("Find('привет', 'ив')", 3)]   // Unicode символы
     public Task FuncFindTests(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "FindLast")]
     [InlineData("FindLast('abc', 'a')", 1)]       // Первый символ
     [InlineData("FindLast('abcba', 'b')", 4)]     // Последнее вхождение
     [InlineData("FindLast('abc', 'c')", 3)]       // Последний символ
     [InlineData("FindLast('abcabc', 'bc')", 5)]   // Последняя подстрока
     [InlineData("FindLast('abc', 'd')", null)]    // Не найдено
     [InlineData("FindLast('', 'a')", null)]       // Пустая строка
     [InlineData("FindLast('abc', '')", 4)]        // Поиск пустой подстроки (возвращает длину строки + 1)
     [InlineData("FindLast('aabaa', 'aa')", 4)]    // Последнее вхождение
     [InlineData("FindLast('привет', 'е')", 5)]    // Unicode символы
     public Task FuncFindLastTests(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "Len")]
     [InlineData("Len('')", 0)]               // Пустая строка
     [InlineData("Len('a')", 1)]              // Один символ
     [InlineData("Len(' x   ')", 5)]          // Учитывает пробелы
     [InlineData("Len('abc')", 3)]            // Несколько символов
     [InlineData("Len('привет')", 6)]         // Unicode символы (6 букв)
     [InlineData("Len('😀')", 1)]             // Emoji (1 символ)
     [InlineData("Len('a\tb')", 3)]           // Символ с escape-последовательностью
     [InlineData("Len(null)", null)]          // Null вход
     public Task FuncLenTests(string expr, object? expected) => Test(expr, expected);
     
     
     [Theory(DisplayName = "Split")]
     // Basic splitting
     [InlineData("Split('a,b,c', ',', 1)", "a")]
     [InlineData("Split('a,b,c', ',', 2)", "b")]
     [InlineData("Split('a,b,c', ',', 3)", "c")]

     // Edge positions
     [InlineData("Split('a,b,c', ',', 0)", null)] // Position < 1
     [InlineData("Split('a,b,c', ',', 4)", null)] // Position > part count
     [InlineData("Split('a,b,c', ',', -1)", null)] // Negative position

     // Different delimiters
     [InlineData("Split('one|two|three', '|', 2)", "two")]
     [InlineData("Split('a;b;c', ';', 3)", "c")]
     [InlineData("Split('a b c', ' ', 2)", "b")]

     // Empty/null cases
     [InlineData("Split('', ',', 1)", "")] // Empty input
     [InlineData("Split('a,b,c', '', 1)", "a,b,c")] // Empty delimiter
     [InlineData("Split(null, ',', 1)", null)] // Null input
     [InlineData("Split('a,b,c', null, 1)", null)] // Null delimiter

     // Multi-character delimiters
     [InlineData("Split('a->b->c', '->', 2)", "b")]
     [InlineData("Split('hello...world', '...', 1)", "hello")]
     [InlineData("Split('a,,b,,c', ',,', 2)", "b")]

     // Special characters
     [InlineData("Split('a\tb\tc', '\t', 2)", "b")] // Tab delimiter
     [InlineData("Split('a\nb\nc', '\n', 3)", "c")] // Newline delimiter
     [InlineData("Split('a\\b\\c', '\\', 2)", "b")] // Escape character

     // Unicode support
     [InlineData("Split('привет,мир,да', ',', 2)", "мир")]
     [InlineData("Split('αβγ→δεζ→θη', '→', 2)", "δεζ")]
     public Task FuncSplitPartTests(string expr, object? expected) => Test(expr, expected);
     
     [Theory(DisplayName = "Composite")]
     [InlineData("'  HeLLo World! '.Trim().Lower()", "hello world!")]
     public Task Composite(string expr, object? expected) => Test(expr, expected);
     
}