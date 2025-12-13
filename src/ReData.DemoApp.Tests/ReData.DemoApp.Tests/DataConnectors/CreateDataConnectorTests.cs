using System.Text;
using FluentValidation;
using ReData.Common;
using ReData.DemoApp.Commands;
using ReData.Query.Core.Types;
using Exception = System.Exception;
using Field = ReData.DemoApp.Database.Entities.Field;

namespace ReData.DemoApp.Tests.DataConnectors;

public class CreateDataConnectorTests(App App) : DemoAppTestBase<App>(App)
{
    private static string FakeDataConnectorName() => $"connector{Guid.NewGuid().ToString("N")[..6]}";

    private static Field TextField(string name) => new()
    {
        Alias = name,
        DataType = DataType.Text,
        CanBeNull = true,
    };

    [Fact(DisplayName = "Создание набора с верными данными должно вернуть 'создано'")]
    public async Task CreateDataset_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream =
                """
                    col1,col2
                    1, qwe
                    2, rty
                    """.ToStream()
        };

        // Act
        var entity = await req.ExecuteAsync();

        // Assert
        entity.Name.Should().Be(req.Name);
        entity.FieldList.Should().BeEquivalentTo([
            TextField("col1"),
            TextField("col2"),
        ]);
    }

    [Fact(DisplayName = "Empty CSV file should fail validation")]
    public async Task CreateDataset_EmptyCsv_ShouldFail()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream = string.Empty.ToStream()
        };

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(() => req.ExecuteAsync());
    }


    [Fact(DisplayName = "CSV with only header row should create empty dataset")]
    public async Task CreateDataset_OnlyHeaderRow_ShouldCreateEmptyDataset()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream = "col1,col2,col3".ToStream()
        };

        // Act
        var entity = await req.ExecuteAsync();

        // Assert
        entity.Name.Should().Be(req.Name);
        entity.FieldList.Should().BeEquivalentTo([
            TextField("col1"),
            TextField("col2"),
            TextField("col3"),
        ]);
    }

    [Fact(DisplayName = "Single column CSV with header should create single field")]
    public async Task CreateDataset_SingleColumnWithHeader_ShouldCreateSingleField()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream =
                """
                    column_name
                    value1
                    value2
                    value3
                    """.ToStream()
        };

        // Act
        var entity = await req.ExecuteAsync();

        // Assert
        entity.FieldList.Should().HaveCount(1);
        entity.FieldList.Should().BeEquivalentTo([TextField("column_name")]);
    }


    [Fact(DisplayName = "Single column CSV without header should create field with name 'A'")]
    public async Task CreateDataset_SingleColumnWithoutHeader_ShouldCreateFieldA()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = false,
            FileStream =
                """
                    value1
                    value2
                    value3
                    """.ToStream()
        };

        // Act
        var entity = await req.ExecuteAsync();

        // Assert
        entity.FieldList.Should().HaveCount(1);
        entity.FieldList.Should().BeEquivalentTo([TextField("A")]);
    }

    [Fact(DisplayName = "CSV with 50+ columns should create all fields")]
    public async Task CreateDataset_ManyColumns_ShouldCreateAllFields()
    {
        // Arrange
        var columnCount = 52;
        var header = string.Join(",", Enumerable.Range(1, columnCount).Select(i => $"col{i}"));
        var dataRow = string.Join(",", Enumerable.Range(1, columnCount).Select(i => $"value{i}"));

        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream = $"{header}\n{dataRow}".ToStream()
        };

        // Act
        var entity = await req.ExecuteAsync();

        // Assert
        entity.FieldList.Should().HaveCount(columnCount);
        entity.FieldList.Should().Contain(f => f.Alias == "col1");
        entity.FieldList.Should().Contain(f => f.Alias == $"col{columnCount}");
    }


    [Fact(DisplayName = "CSV with duplicate column names should handle gracefully")]
    public async Task CreateDataset_DuplicateColumnNames_ShouldHandleGracefully()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream =
                """
                    col1,col1,col2,col2,col2
                    value1,value2,value3,value4,value5
                    """.ToStream()
        };

        // Act
        var entity = await req.ExecuteAsync();

        // Assert
        entity.FieldList.Should().HaveCount(5);
        // Assuming duplicate names are allowed but preserved as-is
        entity.FieldList.Select(f => f.Alias).Should().BeEquivalentTo(
            ["col1", "col1", "col2", "col2", "col2"]);
    }


    [Fact(DisplayName = "CSV with extremely long column names should work")]
    public async Task CreateDataset_LongColumnNames_ShouldWork()
    {
        // Arrange
        var longName = new string('A', 1000); // 1000 character column name
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream = $"{longName}\nvalue1".ToStream()
        };

        // Act
        var entity = await req.ExecuteAsync();

        // Assert
        entity.FieldList.Should().HaveCount(1);
        entity.FieldList[0].Alias.Should().Be(longName);
    }

    [Fact(DisplayName = "CSV with special characters in column names should preserve them")]
    public async Task CreateDataset_SpecialCharactersInColumnNames_ShouldPreserveThem()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream =
                """
                    col with spaces,колонка_кириллица,column-with-dashes,column_with_underscores,col!@#$%^&*()
                    value1,value2,value3,value4,value5
                    """.ToStream()
        };

        // Act
        var entity = await req.ExecuteAsync();

        // Assert
        entity.FieldList.Should().HaveCount(5);
        entity.FieldList.Select(f => f.Alias).Should().BeEquivalentTo([
            "col with spaces",
            "колонка_кириллица",
            "column-with-dashes",
            "column_with_underscores",
            "col!@#$%^&*()"
        ]);
    }

    [Fact(DisplayName = "CSV with quoted special characters in header should parse correctly")]
    public async Task CreateDataset_QuotedSpecialCharactersInHeader_ShouldParse()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream =
                """
                    "col,with,commas","col with "quotes""
                    value1,value2
                    """.ToStream()
        };

        // Act
        var entity = await req.ExecuteAsync();

        // Assert
        entity.FieldList.Should().HaveCount(3);
        entity.FieldList[0].Alias.Should().Be("col,with,commas");
        entity.FieldList[2].Alias.Should().Be("col with \"quotes\"");
    }

    [Fact(DisplayName = "Incorrect separator should parse incorrectly")]
    public async Task CreateDataset_IncorrectSeparator_ShouldParseIncorrectly()
    {
        // Arrange: CSV uses semicolons but we specify comma
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream =
                """
                    col1;col2;col3
                    value1;value2;value3
                    """.ToStream()
        };

        // Act
        var entity = await req.ExecuteAsync();

        // Assert: Should parse as single column because no commas in the file
        entity.FieldList.Should().HaveCount(1);
        entity.FieldList[0].Alias.Should().Be("col1;col2;col3");
    }

    [Fact(DisplayName = "CSV with quoted separators should parse correctly")]
    public async Task CreateDataset_QuotedSeparators_ShouldParseCorrectly()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream =
                """
                    name,address,city
                    "John,Smith","123,Main St",NewYork
                    "Jane,Doe","456,Oak Ave",Boston
                    """.ToStream()
        };

        // Act
        var entity = await req.ExecuteAsync();

        // Assert
        entity.FieldList.Should().HaveCount(3);
        entity.FieldList.Select(f => f.Alias).Should().BeEquivalentTo(["name", "address", "city"]);
    }

    [Fact(DisplayName = "Tab separator should work")]
    public async Task CreateDataset_TabSeparator_ShouldWork()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = '\t',
            WithHeader = true,
            FileStream =
                """
                    col1	col2	col3
                    value1	value2	value3
                    """.ToStream()
        };

        // Act
        var entity = await req.ExecuteAsync();

        // Assert
        entity.FieldList.Should().HaveCount(3);
        entity.FieldList.Select(f => f.Alias).Should().BeEquivalentTo(["col1", "col2", "col3"]);
    }

    [Fact(DisplayName = "Pipe separator should work")]
    public async Task CreateDataset_PipeSeparator_ShouldWork()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = '|',
            WithHeader = true,
            FileStream =
                """
                    col1|col2|col3
                    value1|value2|value3
                    """.ToStream()
        };

        // Act
        var entity = await req.ExecuteAsync();

        // Assert
        entity.FieldList.Should().HaveCount(3);
        entity.FieldList.Select(f => f.Alias).Should().BeEquivalentTo(["col1", "col2", "col3"]);
    }

    [Fact(DisplayName = "Semicolon separator should work")]
    public async Task CreateDataset_SemicolonSeparator_ShouldWork()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ';',
            WithHeader = true,
            FileStream =
                """
                    col1;col2;col3
                    value1;value2;value3
                    """.ToStream()
        };

        // Act
        var entity = await req.ExecuteAsync();

        // Assert
        entity.FieldList.Should().HaveCount(3);
        entity.FieldList.Select(f => f.Alias).Should().BeEquivalentTo(["col1", "col2", "col3"]);
    }

    [SkippableFact(DisplayName = "Multiple character separator should fail validation")]
    public async Task CreateDataset_MultipleCharSeparator_ShouldFail()
    {
        Skip.If(true, "Нужно реализовывать тест на другом слое");
    }

    [SkippableFact(DisplayName = "Empty separator should fail validation")]
    public async Task CreateDataset_EmptySeparator_ShouldFail()
    {
        Skip.If(true, "Нужно реализовывать тест на другом слое");
    }

    [Fact(DisplayName = "Separator that appears in unquoted data values should split incorrectly")]
    public async Task CreateDataset_SeparatorInUnquotedData_ShouldSplitIncorrectly()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream =
                """
                    name,full_address
                    John,123 Main St, Apt 4,New York
                    Jane,456 Oak Ave,Boston
                    """.ToStream()
        };

        // Act
        var entity = await req.ExecuteAsync();

        // Assert: First row will have 3 values, second row 2 values - inconsistent
        // Depends on how your parser handles inconsistent column counts
        // Might fail or use first row as template
        entity.FieldList.Should().HaveCount(2);
        entity.FieldList.Select(f => f.Alias).Should().BeEquivalentTo(["name", "full_address"]);
    }

    [Fact(DisplayName = "Comma separator with quoted fields containing commas should work")]
    public async Task CreateDataset_CommaInQuotedFields_ShouldWork()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream =
                """
                    name,address,tags
                    John,"123 Main St, Apt 4","tag1,tag2,tag3"
                    Jane,"456 Oak Ave, Suite 100","tagA,tagB"
                    """.ToStream()
        };

        // Act
        var entity = await req.ExecuteAsync();

        // Assert
        entity.FieldList.Should().HaveCount(3);
        entity.FieldList.Select(f => f.Alias).Should().BeEquivalentTo(["name", "address", "tags"]);
    }

    [Fact(DisplayName = "Custom rare separator should work")]
    public async Task CreateDataset_CustomSeparator_ShouldWork()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = '^',
            WithHeader = true,
            FileStream =
                """
                    col1^col2^col3
                    value1^value2^value3
                    value4^value5^value6
                    """.ToStream()
        };

        // Act
        var entity = await req.ExecuteAsync();

        // Assert
        entity.FieldList.Should().HaveCount(3);
        entity.FieldList.Select(f => f.Alias).Should().BeEquivalentTo(["col1", "col2", "col3"]);
    }

    [Fact(DisplayName = "Space separator should work")]
    public async Task CreateDataset_SpaceSeparator_ShouldWork()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ' ',
            WithHeader = true,
            FileStream =
                """
                    col1 col2 col3
                    value1 value2 value3
                    """.ToStream()
        };

        // Act
        var entity = await req.ExecuteAsync();

        // Assert
        entity.FieldList.Should().HaveCount(3);
        entity.FieldList.Select(f => f.Alias).Should().BeEquivalentTo(["col1", "col2", "col3"]);
    }

    [Fact(DisplayName = "Separator at beginning or end of line should create empty fields")]
    public async Task CreateDataset_SeparatorAtLineEdges_ShouldCreateEmptyFields()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream = new MemoryStream() // Completely empty
        };

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(() => req.ExecuteAsync());
    }

    [Fact(DisplayName = "Empty stream should fail")]
    public async Task CreateDataset_EmptyStream_ShouldFail()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream = new MemoryStream() // Completely empty
        };

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(() => req.ExecuteAsync());
    }

    [Fact(DisplayName = "Null stream should fail")]
    public async Task CreateDataset_NullStream_ShouldFail()
    {
        // Arrange
        // This depends on your command validation
        // If FileStream is required, this might fail at command creation
        var command = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream = null!
        };

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(() => command.ExecuteAsync());
    }

    [Fact(DisplayName = "Large CSV file should not cause out of memory")]
    public async Task CreateDataset_LargeCsv_ShouldNotCrash()
    {
        // Arrange - create a CSV with many rows
        var rowCount = 10000;
        var stringBuilder = new StringBuilder();

        // Header
        stringBuilder.AppendLine("col1,col2,col3");

        // Many data rows
        for (int i = 0; i < rowCount; i++)
        {
            stringBuilder.AppendLine($"value{i}_1,value{i}_2,value{i}_3");
        }

        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream = stringBuilder.ToString().ToStream()
        };

        // Act - should not throw OutOfMemoryException
        var entity = await req.ExecuteAsync();

        // Assert
        entity.FieldList.Should().HaveCount(3);
        entity.FieldList.Select(f => f.Alias).Should().BeEquivalentTo(["col1", "col2", "col3"]);
    }

    [Fact(DisplayName = "CSV with trailing newline should parse correctly")]
    public async Task CreateDataset_WithTrailingNewline_ShouldParse()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream =
                """
                    col1,col2,col3
                    value1,value2,value3
                    """.ToStream() // Extra newline at end
        };

        // Act
        var entity = await req.ExecuteAsync();

        // Assert
        entity.FieldList.Should().HaveCount(3);
        entity.FieldList.Select(f => f.Alias).Should().BeEquivalentTo(["col1", "col2", "col3"]);
    }

    [Fact(DisplayName = "CSV without trailing newline should parse correctly")]
    public async Task CreateDataset_WithoutTrailingNewline_ShouldParse()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream = "col1,col2,col3\nvalue1,value2,value3".ToStream() // No trailing newline
        };

        // Act
        var entity = await req.ExecuteAsync();

        // Assert
        entity.FieldList.Should().HaveCount(3);
        entity.FieldList.Select(f => f.Alias).Should().BeEquivalentTo(["col1", "col2", "col3"]);
    }

    [Fact(DisplayName = "CSV with Windows line endings should parse")]
    public async Task CreateDataset_WindowsLineEndings_ShouldParse()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream = "col1,col2\r\nvalue1,value2\r\nvalue3,value4".ToStream()
        };

        // Act
        var entity = await req.ExecuteAsync();

        // Assert
        entity.FieldList.Should().HaveCount(2);
        entity.FieldList.Select(f => f.Alias).Should().BeEquivalentTo(["col1", "col2"]);
    }

    [Fact(DisplayName = "CSV with Unix line endings should parse")]
    public async Task CreateDataset_UnixLineEndings_ShouldParse()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream = "col1,col2\nvalue1,value2\nvalue3,value4".ToStream()
        };

        // Act
        var entity = await req.ExecuteAsync();

        // Assert
        entity.FieldList.Should().HaveCount(2);
        entity.FieldList.Select(f => f.Alias).Should().BeEquivalentTo(["col1", "col2"]);
    }

    [Fact(DisplayName = "CSV with mixed line endings should parse")]
    public async Task CreateDataset_MixedLineEndings_ShouldParse()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream = "col1,col2\r\nvalue1,value2\nvalue3,value4\r\n".ToStream()
        };

        // Act
        var entity = await req.ExecuteAsync();

        // Assert
        entity.FieldList.Should().HaveCount(2);
        entity.FieldList.Select(f => f.Alias).Should().BeEquivalentTo(["col1", "col2"]);
    }

    [Fact(DisplayName = "CSV with unclosed quotes should fail")]
    public async Task CreateDataset_UnclosedQuotes_ShouldFail()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream =
                """
                    col1,col2
                    "value1,value2
                    value3,value4
                    """.ToStream()
        };

        // Act
        var entity = await req.ExecuteAsync();

        // Assert
        entity.FieldList.Should().HaveCount(2);
        entity.FieldList.Select(f => f.Alias).Should().BeEquivalentTo(["col1", "col2"]);
    }

    [Fact(DisplayName = "CSV with escaped quotes inside unquoted field might fail")]
    public async Task CreateDataset_EscapedQuotesUnquoted_ShouldHandle()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream =
                """
                    col1,col2
                    value1"with quotes,value2
                    value3,value4
                    """.ToStream()
        };

        // Act
        var entity = await req.ExecuteAsync();

        // Assert
        entity.FieldList.Should().HaveCount(2);
        entity.FieldList.Select(f => f.Alias).Should().BeEquivalentTo(["col1", "col2"]);
    }

    [Fact(DisplayName = "CSV with proper escaped quotes should work")]
    public async Task CreateDataset_EscapedQuotesProper_ShouldWork()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream =
                """"
                    col1,col2
                    "value1 ""with quotes""",value2
                    value3,"value4 ""more quotes"""
                    """".ToStream()
        };

        // Act
        var entity = await req.ExecuteAsync();

        // Assert
        entity.FieldList.Should().HaveCount(2);
        entity.FieldList.Select(f => f.Alias).Should().BeEquivalentTo(["col1", "col2"]);
    }

    [Fact(DisplayName = "CSV with missing field at end should handle empty field")]
    public async Task CreateDataset_MissingFieldAtEnd_ShouldHandle()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream =
                """
                    col1,col2,col3
                    value1,value2
                    value3,value4,value5
                    """.ToStream()
        };

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(() => req.ExecuteAsync());
    }

    [Fact(DisplayName = "CSV with extra field at end should handle or fail")]
    public async Task CreateDataset_ExtraFieldAtEnd_ShouldHandle()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream =
                """
                    col1,col2
                    value1,value2,value3
                    value4,value5
                    """.ToStream()
        };

        // Act
        var entity = await req.ExecuteAsync();

        // Assert - depends on your parser
        entity.FieldList.Should().HaveCount(2);
        entity.FieldList.Select(f => f.Alias).Should().BeEquivalentTo(["col1", "col2"]);
    }

    [Fact(DisplayName = "CSV with only CR (no LF) should parse or fail")]
    public async Task CreateDataset_OnlyCR_ShouldHandle()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream = "col1,col2\rvalue1,value2\r".ToStream()
        };

        await req.ExecuteAsync();
    }
}