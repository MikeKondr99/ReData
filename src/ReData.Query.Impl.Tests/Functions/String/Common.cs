using ReData.Query.Impl.Tests.Fixtures;

namespace ReData.Query.Impl.Tests.Functions.String;

public abstract class Сommon(IDatabaseFixture runner) : ExprTests(runner)
{
    [Theory(DisplayName = "String Addition")]
    [InlineData("'hello' + 'world'", "helloworld")] // Basic concatenation
    [InlineData("'a' + 'b' + 'c'", "abc")] // Multiple concatenations
    [InlineData("'' + 'text'", "text")] // Empty string + text
    [InlineData("'text' + ''", "text")] // Text + empty string
    [InlineData("'' + ''", "")] // Both empty
    // Null handling
    [InlineData("'text' + null", null)] // Text + null
    [InlineData("null + 'text'", null)] // Null + text
    [InlineData("null + null", null)] // Both null
    // Type inference
    [InlineData("Type('a' + 'b')", "text!")] // Both strings → Text
    [InlineData("Type('a' + null)", "text")] // String + null → Text?
    [InlineData("Type(null + 'b')", "text")] // Null + string → Text?
    // Whitespace and special characters
    [InlineData("'hello ' + 'world'", "hello world")] // With spaces
    [InlineData("'line1\\n' + 'line2'", "line1\nline2")] // With newline
    [InlineData("'tab\\t' + 'end'", "tab\tend")] // With tab
    [InlineData("'привет' + 'мир'", "приветмир")] // Unicode
    [InlineData("'😀' + '👍'", "😀👍")] // Emoji
    // With explicit conversions
    [InlineData("'number: ' + Text(42)", "number: 42")] // String + converted number
    [InlineData("Text(3.14) + ' is pi'", "3.14 is pi")] // Converted number + string
    [InlineData("'result: ' + Text(true)", "result: true")] // String + converted boolean
    [InlineData("Text(false) + ' is false'", "false is false")] // Converted boolean + string
    [InlineData("'date: ' + Text(Date(2023,5,15))", "date: 2023-05-15 00:00:00")] // String + converted date
    // Type inference with conversions
    [InlineData("Type('text' + Text(42))", "text!")] // String + converted number → Text
    [InlineData("Type(Text(42) + 'text')", "text!")] // Converted number + string → Text
    // Null handling with conversions
    [InlineData("'text' + Text(null)", null)] // String + converted null
    [InlineData("Text(null) + 'text'", null)] // Converted null + string
    public Task StringAddition(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "Upper")]
    [InlineData("Upper('Hello World!')", "HELLO WORLD!")]
    public Task FuncUpperTetss(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "Lower")]
    [InlineData("Lower('Hello World!')", "hello world!")]
    public Task FuncLowerTests(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "Trim")]
    [InlineData("Trim('  hello  ')", "hello")]
    [InlineData("Trim('  ')", "")]
    [InlineData("Trim('')", "")]
    [InlineData("Trim('привет  ')", "привет")]
    [InlineData("Trim('😀  👍  ')", "😀  👍")] // Emoji with spaces
    [InlineData("Trim(null)", null)]
    public Task FuncTrimTests(string expr, object? expected) => Test(expr, expected);

    [SkippableTheory(DisplayName = "Trim with charset")]
    [InlineData("Trim('...hello...', '.')", "hello")]
    [InlineData("Trim('**hello**', '*')", "hello")]
    [InlineData("Trim('xxhelloxx', 'x')", "hello")]
    [InlineData("Trim('abchelloabc', 'abc')", "hello")]
    [InlineData("Trim('', 'x')", "")]
    [InlineData("Trim('xxxx', 'x')", "")]
    [InlineData("Trim('hello', null)", null)]
    public Task FuncTrimWithCharsetTests(string expr, object? expected)
    {
        Skip.IfNot(runner.GetDatabaseType() is DatabaseType.PostgreSql or DatabaseType.SqlServer,
            "Trim c набором удаляемых символов реализован только в Postgres и SqlServer");
        return Test(expr, expected);
    }

    [Theory(DisplayName = "TrimLeft")]
    [InlineData("TrimLeft('  hello  ')", "hello  ")]
    [InlineData("TrimLeft('  ')", "")]
    [InlineData("TrimLeft('')", "")]
    [InlineData("TrimLeft('  привет')", "привет")]
    [InlineData("TrimLeft('  😀👍')", "😀👍")]
    [InlineData("TrimLeft(null)", null)]
    public Task FuncTrimLeftTests(string expr, object? expected) => Test(expr, expected);
    
    [SkippableTheory(DisplayName = "TrimLeft with charset")]
    [InlineData("TrimLeft('...hello...', '.')", "hello...")]
    [InlineData("TrimLeft('**hello**', '*')", "hello**")]
    [InlineData("TrimLeft('xxhelloxx', 'x')", "helloxx")]
    [InlineData("TrimLeft('bcahelloabc', 'abc')", "helloabc")]
    [InlineData("TrimLeft('', 'x')", "")]
    [InlineData("TrimLeft('xxxx', 'x')", "")]
    [InlineData("TrimLeft('hello', null)", null)]
    public Task FuncTrimLeftWithCharsetTests(string expr, object? expected)
    {
        Skip.IfNot(runner.GetDatabaseType() is DatabaseType.PostgreSql or DatabaseType.SqlServer,
            "TrimLeft c набором удаляемых символов реализован только в Postgres и SqlServer");
        return Test(expr, expected);
    }

    [Theory(DisplayName = "TrimRight")]
    [InlineData("TrimRight('  hello  ')", "  hello")]
    [InlineData("TrimRight('  ')", "")]
    [InlineData("TrimRight('')", "")]
    [InlineData("TrimRight('привет  ')", "привет")]
    [InlineData("TrimRight('😀👍  ')", "😀👍")]
    [InlineData("TrimRight(null)", null)]
    public Task FuncTrimRightTests(string expr, object? expected) => Test(expr, expected);
    

    [SkippableTheory(DisplayName = "TrimRight with charset")]
    [InlineData("TrimRight('...hello...', '.')", "...hello")]
    [InlineData("TrimRight('**hello**', '*')", "**hello")]
    [InlineData("TrimRight('xxhelloxx', 'x')", "xxhello")]
    [InlineData("TrimRight('abchelloabc', 'abc')", "abchello")]
    [InlineData("TrimRight('', 'x')", "")]
    [InlineData("TrimRight('xxxx', 'x')", "")]
    [InlineData("TrimRight('hello', null)", null)]
    public Task FuncTrimRightWithCharsetTests(string expr, object? expected)
    {
        Skip.IfNot(runner.GetDatabaseType() is DatabaseType.PostgreSql or DatabaseType.SqlServer,
            "TrimLeft c набором удаляемых символов реализован только в Postgres и SqlServer");
        return Test(expr, expected);
    }
    
    [SkippableTheory(DisplayName = "PadLeft(text, count)")]
    [InlineData("PadLeft('abc', 5)", "  abc")] // Basic padding
    [InlineData("PadLeft('abc', 3)", "abc")] // Same length
    [InlineData("PadLeft('abc', 2)", "ab")] // Shorter than length
    [InlineData("PadLeft('abc', 0)", "")] // Zero count
    [InlineData("PadLeft('', 5)", "     ")] // Empty string
    [InlineData("PadLeft('привет', 8)", "  привет")] // Unicode
    [InlineData("PadLeft('😀', 3)", "  😀")] // Emoji
    [InlineData("PadLeft('abc', -1)", "")] // Negative count
    [InlineData("PadLeft(null, 5)", null)]
    public Task FuncPadLeftCountTests(string expr, object? expected)
    {
        Skip.If(expr.Contains("😀") && runner.GetDatabaseType() is DatabaseType.SqlServer, "Sql server не справляется с сложным юникодом");
        return Test(expr, expected);
    }

    [SkippableTheory(DisplayName = "PadRight(text, count)")]
    [InlineData("PadRight('abc', 5)", "abc  ")] // Basic padding
    [InlineData("PadRight('abc', 3)", "abc")] // Same length
    [InlineData("PadRight('abc', 2)", "ab")] // Shorter than length
    [InlineData("PadRight('abc', 0)", "")] // Zero count
    [InlineData("PadRight('', 5)", "     ")] // Empty string
    [InlineData("PadRight('привет', 8)", "привет  ")] // Unicode
    [InlineData("PadRight('😀', 3)", "😀  ")] // Emoji
    [InlineData("PadRight('abc', -1)", "")] // Negative count
    [InlineData("PadRight(null, 5)", null)]
    public Task FuncPadRightCountTests(string expr, object? expected)
    {
        Skip.If(expr.Contains("😀") && runner.GetDatabaseType() is DatabaseType.SqlServer, "Sql server не справляется с сложным юникодом");
        return Test(expr, expected);
    }

    [Theory(DisplayName = "PadLeft(text, count, symbol)")]
    [InlineData("PadLeft('abc', 5, '*')", "**abc")] // Custom symbol
    [InlineData("PadLeft('abc', 5, '0')", "00abc")] // Zero padding
    [InlineData("PadLeft('abc', 3, '*')", "abc")] // Same length
    [InlineData("PadLeft('abc', 2, '*')", "ab")] // Shorter than length
    [InlineData("PadLeft('abc', 0, '*')", "")] // Zero count
    [InlineData("PadLeft('', 5, '-')", "-----")] // Empty string
    [InlineData("PadLeft('123', 5, '0')", "00123")] // Numeric padding
    [InlineData("PadLeft('abc', 5, '.')", "..abc")] // Dot padding
    [InlineData("PadLeft('abc', 5, 'XY')", "XYabc")] // Multi-char symbol (uses first char)
    [InlineData("PadLeft('abc', 5, '')", "abc")]
    [InlineData("PadLeft('abc', -1, '*')", "")]
    [InlineData("PadLeft(null, 5, '*')", null)]
    public Task FuncPadLeftSymbolTests(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "PadRight(text, count, symbol)")]
    [InlineData("PadRight('abc', 5, '*')", "abc**")] // Custom symbol
    [InlineData("PadRight('abc', 5, '0')", "abc00")] // Zero padding
    [InlineData("PadRight('abc', 3, '*')", "abc")] // Same length
    [InlineData("PadRight('abc', 2, '*')", "ab")] // Shorter than length
    [InlineData("PadRight('abc', 0, '*')", "")] // Zero count
    [InlineData("PadRight('', 5, '-')", "-----")] // Empty string
    [InlineData("PadRight('123', 5, '0')", "12300")] // Numeric padding
    [InlineData("PadRight('abc', 5, '.')", "abc..")] // Dot padding
    [InlineData("PadRight('abc', 5, 'XY')", "abcXY")] // Multi-char symbol (uses first char)
    [InlineData("PadRight('abc', 5, '')", "abc")]
    [InlineData("PadRight('abc', -1, '*')", "")] // Negative count
    [InlineData("PadRight(null, 5, '*')", null)]
    public Task FuncPadRightSymbolTests(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "Substring")]
    [InlineData("Substring('Hello World!',5)", "o World!")]
    [InlineData("Substring('Hello World!',5,3)", "o W")]
    public Task FuncSubstringTests(string expr, object? expected) => Test(expr, expected);

    [SkippableTheory(DisplayName = "Reverse")]
    [InlineData("Reverse('Hello')", "olleH")]
    [InlineData("Reverse('Привет мир!')", "!рим тевирП")]
    public Task FuncReverseTests(string expr, object? expected)
    {
        Skip.If(expr.Contains('П') && runner.GetDatabaseType() is DatabaseType.Oracle);
        return Test(expr, expected);
    }

    [SkippableTheory(DisplayName = "EmptyIsNull")]
    [InlineData("EmptyIsNull('Hello world!')", "Hello world!")]
    [InlineData("EmptyIsNull('')", null)]
    [InlineData("EmptyIsNull(null)", null)]
    public Task FuncEmptyIsNullTests(string expr, object? expected)
    {
        Skip.If(expr is "EmptyIsNull(null)" && runner.GetDatabaseType() is DatabaseType.SqlServer);
        return Test(expr, expected);
    }

    [SkippableTheory(DisplayName = "Replace")]
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
    public Task FuncReplaceTests(string expr, object? expected)
    {
        Skip.If(expr.StartsWith("Replace(''") && runner.GetDatabaseType() is DatabaseType.Oracle);
        return Test(expr, expected);
    }
    
    [Theory(DisplayName = "Repeat")]
    [InlineData("Repeat('a', 3)", "aaa")] // Базовый случай
    [InlineData("Repeat('ab', 2)", "abab")] // Мультисимвольная строка
    [InlineData("Repeat(' ', 4)", "    ")] // Пробелы
    [InlineData("Repeat('', 5)", "")] // Пустая строка
    [InlineData("Repeat('a', 0)", "")] // Ноль повторений
    [InlineData("Repeat('a', 1)", "a")] // Одно повторение
    [InlineData("Repeat('привет', 2)", "приветпривет")] // Unicode
    [InlineData("Repeat('😀', 3)", "😀😀😀")] // Emoji
    [InlineData("Repeat('\\n', 2)", "\n\n")] // Escape-символы
    [InlineData("Repeat('.*+', 2)", ".*+.*+")] // Символы regex
    [InlineData("Repeat('a', -1)", "")] // Отрицательный count
    [InlineData("Repeat('abc', 1000).Len()", 3000)] // Большой count (проверка длины)
    [InlineData("Repeat(null, 3)", null)]
    [InlineData("Repeat('abc', null)", null)]
    public Task FuncRepeatTests(string expr, object? expected) => Test(expr, expected);

    [SkippableTheory(DisplayName = "Index")]
    [InlineData("Index('abc', 'a')", 1)] // Первый символ
    [InlineData("Index('abc', 'b')", 2)] // Середина строки
    [InlineData("Index('abc', 'c')", 3)] // Последний символ
    [InlineData("Index('abc', 'bc')", 2)] // Подстрока
    [InlineData("Index('aaaaAaaa', 'A')", 5)] // Поиск чувствителен к регистру
    [InlineData("Index('abc', 'd')", null)] // Не найдено
    [InlineData("Index('', 'a')", null)] // Пустая строка
    [InlineData("Index('abc', '')", 1)] // Поиск пустой подстроки
    [InlineData("Index('aabaa', 'aa')", 1)] // Первое вхождение
    [InlineData("Index('привет', 'ив')", 3)] // Unicode символы
    public Task FuncIndexTests(string expr, object? expected)
    {
        Skip.If(runner.GetDatabaseType() is DatabaseType.Oracle && expr.Contains("''"));
        return Test(expr, expected);
    }

    [SkippableTheory(DisplayName = "LastIndex")]
    [InlineData("LastIndex('abc', 'a')", 1)] // Первый символ
    [InlineData("LastIndex('abcba', 'b')", 4)] // Последнее вхождение
    [InlineData("LastIndex('abc', 'c')", 3)] // Последний символ
    [InlineData("LastIndex('abcabc', 'bc')", 5)] // Последняя подстрока
    [InlineData("Index('aaaaAaaa', 'A')", 5)] // Поиск чувствителен к регистру
    [InlineData("LastIndex('abc', 'd')", null)] // Не найдено
    [InlineData("LastIndex('', 'a')", null)] // Пустая строка
    [InlineData("LastIndex('abc', '')", 4)] // Поиск пустой подстроки (возвращает длину строки + 1)
    [InlineData("LastIndex('aabaa', 'aa')", 4)] // Последнее вхождение
    [InlineData("LastIndex('привет', 'е')", 5)] // Unicode символы
    public Task FuncLastIndexTests(string expr, object? expected)
    {
        Skip.If(runner.GetDatabaseType() is DatabaseType.Oracle && (expr.Contains("''") || expr.Contains('е')));
        return Test(expr, expected);
    }

    [SkippableTheory(DisplayName = "Len")]
    [InlineData("Len('')", 0)] // Пустая строка
    [InlineData("Len('a')", 1)] // Один символ
    [InlineData("Len(' x   ')", 5)] // Учитывает пробелы
    [InlineData("Len('abc')", 3)] // Несколько символов
    [InlineData("Len('привет')", 6)] // Unicode символы (6 букв)
    [InlineData("Len('😀')", 1)] // Emoji (1 символ)
    [InlineData("Len('a\tb')", 3)] // Символ с escape-последовательностью
    [InlineData("Len(null)", null)] // Null вход
    public Task FuncLenTests(string expr, object? expected)
    {
        Skip.If(expr is "Len('')" && runner.GetDatabaseType() is DatabaseType.Oracle,
            "Oracle не видит разницы между '' и null");
        Skip.If(expr is "Len('😀')" && runner.GetDatabaseType() is DatabaseType.SqlServer,
            "Не выходит научить работать SqlServer с юникодом");
        return Test(expr, expected);
    }

    [Theory(DisplayName = "Contains")]
    [InlineData("'hello'.Contains('ell')", true)]
    [InlineData("Contains('hello','world')", false)]
    [InlineData("'hello'.Contains('')", true)] // Пустая подстрока всегда найдена
    [InlineData("''.Contains('a')", false)] // Пустая строка не содержит непустую подстроку
    [InlineData("''.Contains('')", true)] // Пустая строка содержит пустую подстроку
    [InlineData("Contains('привет','иве')", true)] // Русские буквы
    [InlineData("'😀👍👋'.Contains('👍')", true)] // Emoji
    [InlineData("'Apple'.Contains('P')", false)] // Case sensitive
    [InlineData("'Apple'.Contains('p')", true)] // Case sensitive
// [InlineData("'text'.Contains(null)", null)] // null подстрока
// [InlineData("null.Contains('abc')", null)] // null строка
    public Task FuncContainsTests(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "StartsWith")]
    [InlineData("'hello'.StartsWith('hel')", true)]
    [InlineData("StartsWith('hello','world')", false)]
    [InlineData("'hello'.StartsWith('')", true)] // Пустая подстрока всегда в начале
    [InlineData("''.StartsWith('a')", false)] // Пустая строка не начинается с непустой подстроки
    [InlineData("''.StartsWith('')", true)] // Пустая строка начинается с пустой подстроки
    [InlineData("StartsWith('привет','при')", true)] // Русские буквы
    [InlineData("'😀👍👋'.StartsWith('😀')", true)] // Emoji
    [InlineData("'Apple'.StartsWith('A')", true)] // Case sensitive
    [InlineData("'Apple'.StartsWith('a')", false)] // Case sensitive
// [InlineData("'text'.StartsWith(null)", null)] // null подстрока
// [InlineData("null.StartsWith('abc')", null)] // null строка
    public Task FuncStartsWithTests(string expr, object? expected) => Test(expr, expected);

    [Theory(DisplayName = "EndsWith")]
    [InlineData("'hello'.EndsWith('llo')", true)]
    [InlineData("EndsWith('hello','world')", false)]
    [InlineData("'hello'.EndsWith('')", true)] // Пустая подстрока всегда в конце
    [InlineData("''.EndsWith('a')", false)] // Пустая строка не заканчивается непустой подстрокой
    [InlineData("''.EndsWith('')", true)] // Пустая строка заканчивается пустой подстрокой
    [InlineData("EndsWith('привет','вет')", true)] // Русские буквы
    [InlineData("'😀👍👋'.EndsWith('👋')", true)] // Emoji
    [InlineData("'Apple'.EndsWith('e')", true)] // Case sensitive
    [InlineData("'Apple'.EndsWith('E')", false)] // Case sensitive
// [InlineData("'text'.EndsWith(null)", null)] // null подстрока
// [InlineData("null.EndsWith('abc')", null)] // null строка
    public Task FuncEndsWithTests(string expr, object? expected) => Test(expr, expected);

    [SkippableTheory(DisplayName = "Split")]
    [InlineData("Split('a,b,c', ',', 1).Type()", "text")] // Always nullable text
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
    [InlineData(@"Split('a\tb\tc', '\t', 2)", "b")] // Tab delimiter
    [InlineData(@"Split('a\nb\nc', '\n', 3)", "c")] // Newline delimiter
    [InlineData(@"Split('a\\b\\c', '\\', 2)", "b")] // Escape character

    // Unicode support
    [InlineData("Split('привет,мир,да', ',', 2)", "мир")]
    [InlineData("Split('αβγ→δεζ→θη', '→', 2)", "δεζ")]
    public Task FuncSplitTests(string expr, object? expected)
    {
        Skip.If(runner.GetDatabaseType() is not  DatabaseType.PostgreSql, $"{runner.GetDatabaseType()} не имеет реализацию функции Split");
        return Test(expr, expected);
    }
    
    [SkippableTheory(DisplayName = "ExcludeChars")]
    [InlineData("ExcludeChars('a1b2c3', '123')", "abc")] // Удаление цифр
    [InlineData("ExcludeChars('hello world', 'aeiou')", "hll wrld")] // Удаление гласных
    [InlineData("ExcludeChars('привет123', '123')", "привет")] // Unicode с цифрами
    [InlineData("ExcludeChars('abc123', '')", "abc123")] // Пустая строка символов для удаления
    [InlineData("ExcludeChars('', '123')", "")] // Пустая исходная строка
    [InlineData("ExcludeChars('aaaa', 'a')", "")] // Все символы удаляются
    [InlineData("ExcludeChars('abc', 'xyz')", "abc")] // Нет совпадений
    [InlineData("ExcludeChars('aAbBcC', 'A')", "abBcC")] // Case sensitive
    [InlineData("ExcludeChars('😀a👍b', 'a')", "😀👍b")] // Emoji и текст
    [InlineData("ExcludeChars('a b c', ' ')", "abc")] // Удаление пробелов
    [InlineData("ExcludeChars(null, '123')", null)]
    [InlineData("ExcludeChars('abc', null)", null)]
    public Task FuncExcludeCharsTests(string expr, object? expected)
    {
        Skip.If(runner.GetDatabaseType() is not  DatabaseType.PostgreSql, $"{runner.GetDatabaseType()} не имеет реализацию функции ExcludeChars");
        return Test(expr, expected);
    }

    [SkippableTheory(DisplayName = "KeepChars")]
    [InlineData("KeepChars('a1b2c3', '123')", "123")] // Основной пример
    [InlineData("KeepChars('hello world', 'aeiou')", "eoo")] // Оставляем гласные
    [InlineData("KeepChars('привет123', '123')", "123")] // Unicode с цифрами
    [InlineData("KeepChars('abc123', '')", "")] // Пустая строка символов для оставления
    [InlineData("KeepChars('', '123')", "")] // Пустая исходная строка
    [InlineData("KeepChars('aaaa', 'a')", "aaaa")] // Все символы остаются
    [InlineData("KeepChars('abc', 'xyz')", "")] // Нет совпадений
    [InlineData("KeepChars('aAbBcC', 'A')", "A")] // Case sensitive
    [InlineData("KeepChars('😀a👍b', 'a')", "a")] // Emoji и текст (оставляем только букву)
    [InlineData("KeepChars('a b c', 'abc')", "abc")] // Удаление пробелов, оставляем буквы
    [InlineData("KeepChars('1a2b3c', '1234567890')", "123")] // Оставляем только цифры
    [InlineData("KeepChars(null, '123')", null)]
    [InlineData("KeepChars('abc', null)", null)]
    public Task FuncKeepCharsTests(string expr, object? expected)
    {
        Skip.If(runner.GetDatabaseType() is not  DatabaseType.PostgreSql, $"{runner.GetDatabaseType()} не имеет реализацию функции ExcludeChars");
        return Test(expr, expected);
    }
    
    [SkippableTheory(DisplayName = "Chr")]
    [InlineData("Chr(65)", "A")] // ASCII uppercase A
    [InlineData("Chr(97)", "a")] // ASCII lowercase a
    [InlineData("Chr(32)", " ")] // Space
    [InlineData("Chr(9)", "\t")] // Tab
    [InlineData("Chr(10)", "\n")] // Newline
    [InlineData("Chr(13)", "\r")] // Carriage return
    [InlineData("Chr(0)", "\0")] // Null character
    [InlineData("Chr(255)", "ÿ")] // Extended ASCII

    // Unicode characters (Cyrillic)
    [InlineData("Chr(1055)", "П")] // Cyrillic capital Pe
    [InlineData("Chr(1087)", "п")] // Cyrillic small pe
    [InlineData("Chr(1040)", "А")] // Cyrillic capital A
    [InlineData("Chr(1072)", "а")] // Cyrillic small a

    // Unicode characters beyond Basic Multilingual Plane
    [InlineData("Chr(128512)", "😀")] // Emoji: grinning face (U+1F600)
    [InlineData("Chr(128077)", "👍")] // Emoji: thumbs up (U+1F44D)
    [InlineData("Chr(128075)", "👋")] // Emoji: waving hand (U+1F44B)

    // Common symbols
    [InlineData("Chr(8364)", "€")] // Euro sign
    [InlineData("Chr(163)", "£")] // Pound sign
    [InlineData("Chr(165)", "¥")] // Yen sign
    [InlineData("Chr(169)", "©")] // Copyright

    // Edge cases
    [InlineData("Chr(-1)", null)] // Negative code
    [InlineData("Chr(1114112)", null)] // Beyond Unicode range (max is 0x10FFFF = 1114111)
    [InlineData("Chr(null)", null)] // Null input

    // Type checking
    // [InlineData("Type(Chr(65))", "text")] // Always nullable because invalid codes return null
    // [InlineData("Type(Chr(-1))", "text")] // Invalid code also returns nullable text
    public Task FuncChrTests(string expr, object? expected)
    {
        Skip.If(runner.GetDatabaseType() is DatabaseType.PostgreSql && expr is "Chr(0)",
            "PostgreSQL не поддерживает \\0 в text");
        return Test(expr, expected);
    }
    
}
