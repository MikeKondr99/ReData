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
    // Базовые строки
    [InlineData("'Hello World!'", "Hello World!")]
    [InlineData("'contains $ inside'", "contains $ inside")]
    [InlineData("''", "")] // Пустая строка

    // Эскейп последовательности
    [InlineData(@"'line1\nline2'", "line1\nline2")] // Перенос строки
    [InlineData(@"'\\n'", @"\n")] // Буквально \n (не перенос)
    [InlineData(@"'tab\tseparated'", "tab\tseparated")] // Табуляция
    [InlineData(@"'caret\rreturn'", "caret\rreturn")] // Возврат каретки
    [InlineData(@"'backslash\\here'", @"backslash\here")] // Бэкслеш
    [InlineData(@"'just \p'", @"just \p")] // Неизвестный эскейп (остаётся как есть)
    [InlineData(@"' \\\\\\ '", @" \\\ ")] // Множественные бэкслеши
    [InlineData(@"'quoted: \'text\''", "quoted: 'text'")] // Эскейп кавычек

    // Unicode
    [InlineData("'unicode: привет'", "unicode: привет")]
    [InlineData("'emoji: 😀👍'", "emoji: 😀👍")]
    [InlineData("'café'", "café")] // Latin-1 Supplement
    [InlineData("'北京'", "北京")] // CJK иероглифы

    // Интерполяция
    [InlineData("'value: ${42}'", "value: 42")] // Число в интерполяции
    [InlineData("'text: ${'hello'}'", "text: hello")] // Текст в интерполяции
    [InlineData("'bool: ${true}'", "bool: true")] // Булево в интерполяции
    [InlineData("'null: ${null}'", null)] // Null в интерполяции = null
    [InlineData("'math: ${2+2}'", "math: 4")] // Выражение в интерполяции
    [InlineData("'nested: ${Text(123)}'", "nested: 123")] // Функция в интерполяции
    [InlineData("'today: ${Date(2023,5,15)}'", "today: 2023-05-15 00:00:00")] // Дата в интерполяции

    // Эскейп интерполяции
    [InlineData(@"'\${expr}'", "${expr}")] // Эскейп интерполяции
    [InlineData(@"'start \${expr} middle ${42} end'", "start ${expr} middle 42 end")] // Смешанный случай
    [InlineData(@"'\\${42}'", @"\42")] // Бэкслеш перед интерполяцией
    [InlineData(@"'\\\\${42}'", @"\\42")] // Двойной бэкслеш + интерполяция

    // Сложные случаи с эскейпами
    [InlineData(@"'\n${'inter'}\tpolated\r'", "\ninter\tpolated\r")] // Эскейпы + интерполяция
    [InlineData(@"'\\${'test'}\\n'", @"\test\n")] // Бэкслеш + интерполяция + буквенный \n
    [InlineData(@"'\'${'quoted'}\''", "'quoted'")] // Эскейп кавычек с интерполяцией

    // Безопасность SQL
    [InlineData(@"'; DROP TABLE users; --'", "; DROP TABLE users; --")] // SQL инъекция (должно остаться как текст)
    [InlineData(@"'\' OR 1=1; --'", "' OR 1=1; --")] // SQL инъекция с кавычкой
    [InlineData(@"'${'; DROP TABLE'}'", "; DROP TABLE")] // SQL инъекция в интерполяции

    // Специальные символы SQL
    [InlineData(@"'%_%'", "%_%")] // LIKE wildcards (должны остаться как есть)
    [InlineData(@"'[](){}|+*?^$.'", "[](){}|+*?^$.")] // Регекс символы
    public Task Text(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "Bool")]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public Task Bool(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "Null")]
    [InlineData("'' + null", null)]
    public Task Null(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "Contants (Literal Script)")]
    [InlineData("const a = 10; a", 10)]
    [InlineData("const a = 10; const b = 2; a + b", 12)]
    [InlineData("const a = 10; const b = 2; a - b", 8)]
    [InlineData("const a = 10; const b = 2; a * b", 20)]
    [InlineData("const a = 10; If(a = 10, 1, 0)", 1)]
    [InlineData("const a = 11; If(a > 5, 1, 0)", 1)]
    [InlineData("const a = 11; If(a < 5, 1, 0)", 0)]
    [InlineData("const a = true; If(a,1,0)", 1)]
    [InlineData("const a = false; If(a,1,0)", 0)]
    [InlineData("const a = null; If(IsNull(a), 1, 0)", 1)]
    [InlineData("const a = 'hi'; a", "hi")]
    [InlineData("const a = 'hi'; const b = ' there'; a + b", "hi there")]
    [InlineData("const a = 'abc'; Len(a)", 3)]
    [InlineData("const a = 'Hello'; Lower(a)", "hello")]
    [InlineData("const a = 'Hello'; Upper(a)", "HELLO")]
    [InlineData("const a = '  hi  '; Trim(a)", "hi")]
    [InlineData("const a = 'abc'; If(a.Contains('b'), 1, 0)", 1)]
    [InlineData("const a = 'abc'; If(a.StartsWith('a'), 1, 0)", 1)]
    [InlineData("const a = 'abc'; If(a.EndsWith('c'), 1, 0)", 1)]
    [InlineData("const a = 7; Type(a)", "int!")]
    [InlineData("const a = null; Type(a)", "null")]
    [InlineData("const a = 5; const b = 5; If(a = b, 1, 0)", 1)]
    [InlineData("const a = 5; const b = 8; If(a = b, 1, 0)", 0)]
    [InlineData("const a = 1; const b = 2; const c = 3; a + b + c", 6)]
    [InlineData("const a = 'привет'; a + ' мир'", "привет мир")]
    public Task ConstantsLiteralScript(string expr, object? expected) => Test(expr, expected);
}
