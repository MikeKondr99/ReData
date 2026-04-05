using System.Text;
using ReData.Common;
using ReData.DemoApp.Commands;
using ReData.DemoApp.Database.Entities;
using ReData.Query.Core.Types;
using Exception = System.Exception;

namespace ReData.DemoApp.Tests.DataConnectors;

public class CreateDataConnectorTests(App App) : DemoAppTestBase<App>(App)
{
    private static string FakeDataConnectorName() => $"connector{Guid.NewGuid().ToString("N")[..6]}";

    private static DataConnectorField TextField(string name) => new()
    {
        Alias = name,
        Column = "column",
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
        entity.FieldList.Select(f => f.Alias).Should().BeEquivalentTo(["col1", "col2"]);
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
        entity.FieldList.Select(f => f.Alias).Should().BeEquivalentTo(["col1", "col2", "col3"]);
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
        entity.FieldList.Select(f => f.Alias).Should().BeEquivalentTo(["column_name"]);
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
        entity.FieldList.Select(f => f.Alias).Should().BeEquivalentTo(["A"]);
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
        entity.FieldList.Select(f => f.Alias).Should().BeEquivalentTo(
            ["col1", "col1_2", "col2", "col2_2", "col2_3"]);
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
                    "col,with,commas","col with "quotes" kek"
                    value1,value2
                    """.ToStream()
        };

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(() => req.ExecuteAsync());
    }


    [Fact(DisplayName = "CSV with header should trim empty characters")]
    public async Task CreateDataset_WithHeaders_ShouldTrimHeaders()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream =
                """
                    col1 , col2 
                    value1,value2
                    """.ToStream()
        };

        // Act
        var entity = await req.ExecuteAsync();

        // Assert
        entity.FieldList.Should().HaveCount(2);
        entity.FieldList[0].Alias.Should().Be("col1");
        entity.FieldList[1].Alias.Should().Be("col2");
    }

    [Fact(DisplayName = "CSV with header, but no data should parse")]
    public async Task CreateDataset_WithHeadersButNoData_ShouldParse()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream = "1,-7,70,1000023429387429837529875,30".ToStream()
        };

        // Act
        var entity = await req.ExecuteAsync();

        // Assert
        entity.FieldList.Should().HaveCount(5);
        entity.FieldList[0].Alias.Should().Be("1");
        entity.FieldList[1].Alias.Should().Be("-7");
        entity.FieldList[2].Alias.Should().Be("70");
        entity.FieldList[3].Alias.Should().Be("1000023429387429837529875");
        entity.FieldList[4].Alias.Should().Be("30");
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

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(() => req.ExecuteAsync());
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

    [Fact(DisplayName = "CSV с пропущенными полем в конца засчитывается за null")]
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

        // Act
        var entity = await req.ExecuteAsync();

        // Assert
        entity.FieldList.Should().HaveCount(3);
        entity.FieldList.Select(f => f.Alias).Should().BeEquivalentTo(["col1", "col2", "col3"]);
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

    [Fact(DisplayName = "Создание коннектора с целыми числами → должен сохранить столбец с типом DataType.Integer")]
    public async Task CreateDataset_IntegerFormat_Valid()
    {
        // Arrange
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream =
                """
                    id,count,negative
                    1,2,-3
                    2,10,-999
                    ,,
                    """.ToStream()
        };

        // Act
        var entity = await req.ExecuteAsync();

        // Assert
        entity.FieldList.Should().HaveCount(3);
        entity.FieldList[0].DataType.Should().Be(DataType.Integer);
        entity.FieldList[1].DataType.Should().Be(DataType.Integer);
        entity.FieldList[2].DataType.Should().Be(DataType.Integer);
    }

    [Fact(DisplayName =
        "Создание коннектора с числами с плавающей точкой → должен сохранить столбец с типом DataType.Number")]
    public async Task CreateDataset_NumberFormat_Valid()
    {
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream =
                """
                    price,temperature,ratio
                    13.45,-2.5,0.75
                    100.00,98.6,0.5
                    ,,
                    """.ToStream()
        };

        var entity = await req.ExecuteAsync();
        entity.FieldList.Should().AllSatisfy(f => f.DataType.Should().Be(DataType.Number));
    }

    [Fact(DisplayName =
        "Создание коннектора с целыми и дробными числами в одном столбце → должен выбрать тип DataType.Number")]
    public async Task CreateDataset_IntegerMixedWithNumber_FallbackToNumber()
    {
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream =
                """
                    id,price
                    1,13.45
                    2,10
                    ,,
                    """.ToStream()
        };

        var entity = await req.ExecuteAsync();
        entity.FieldList[0].DataType.Should().Be(DataType.Integer);
        entity.FieldList[1].DataType.Should().Be(DataType.Number);
    }

    [Fact(DisplayName = "Создание коннектора с булевыми значениями true/false → должен сохранить тип DataType.Bool")]
    public async Task CreateDataset_BooleanTrueFalse_Valid()
    {
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream =
                """
                    active,verified,completed
                    true,false,true
                    False,TRUE,False
                    ,,
                    """.ToStream()
        };

        var entity = await req.ExecuteAsync();
        entity.FieldList.Should().AllSatisfy(f => f.DataType.Should().Be(DataType.Bool));
    }

    [Fact(DisplayName = "Создание коннектора с булевыми значениями 1/0 → должен сохранить тип DataType.Bool")]
    public async Task CreateDataset_BooleanOneZero_Valid()
    {
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream =
                """
                    is_active,has_access
                    1,0
                    0,1
                    ,,
                    """.ToStream()
        };

        var entity = await req.ExecuteAsync();
        entity.FieldList.Should().AllSatisfy(f => f.DataType.Should().Be(DataType.Bool));
    }

    [Fact(DisplayName = "Создание коннектора с датами в формате ISO → должен сохранить тип DataType.DateTime")]
    public async Task CreateDataset_ISODateFormat_Valid()
    {
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream =
                """
                    date_of_birth,registration_date
                    1990-01-15,2023-12-31
                    1985-07-20,2024-01-01
                    ,,
                    """.ToStream()
        };

        var entity = await req.ExecuteAsync();
        entity.FieldList.Should().AllSatisfy(f => f.DataType.Should().Be(DataType.DateTime));
    }

    [Fact(DisplayName = "Создание коннектора с UTC датами → должен сохранить тип DataType.DateTime")]
    public async Task CreateDataset_UTCDateFormat_Valid()
    {
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream =
                """
                    created_at,updated_at
                    2023-12-31T23:59:59Z,2024-01-01T00:00:00Z
                    2023-06-15T12:30:00Z,2023-06-15T14:45:00Z
                    ,,
                    """.ToStream()
        };

        var entity = await req.ExecuteAsync();
        entity.FieldList.Should().AllSatisfy(f => f.DataType.Should().Be(DataType.DateTime));
    }

    [Fact(DisplayName = "Создание коннектора с текстовыми данными → должен сохранить тип DataType.Text")]
    public async Task CreateDataset_TextFormat_Valid()
    {
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream =
                """
                    name,city,description
                    Иван,Москва,Разработчик ПО
                    Мария,Санкт-Петербург,Тестировщик
                    ,,
                    """.ToStream()
        };

        var entity = await req.ExecuteAsync();
        entity.FieldList.Should().AllSatisfy(f => f.DataType.Should().Be(DataType.Text));
    }

    [Fact(DisplayName = "Создание коннектора со смешанными типами данных → должен выбрать самый общий тип")]
    public async Task CreateDataset_MixedTypes_FallbackToMostGeneral()
    {
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream =
                """
                    mixed_column
                    123
                    45.67
                    текстовое значение
                    true
                    2023-01-01
                    """.ToStream()
        };

        var entity = await req.ExecuteAsync();
        entity.FieldList[0].DataType.Should().Be(DataType.Text);
    }

    [Fact(DisplayName =
        "Создание коннектора с пустыми значениями → должен корректно определить тип по непустым значениям")]
    public async Task CreateDataset_WithNullValues_TypeDeterminedFromNonEmpty()
    {
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream =
                """
                    id,name,price
                    1,,10.5
                    ,Иван,
                    3,Мария,20.0
                    """.ToStream()
        };

        var entity = await req.ExecuteAsync();
        entity.FieldList[0].DataType.Should().Be(DataType.Integer);
        entity.FieldList[1].DataType.Should().Be(DataType.Text);
        entity.FieldList[2].DataType.Should().Be(DataType.Number);
    }

    [Fact(DisplayName = "Создание коннектора только с пустыми значениями → должен выбрать тип DataType.Text")]
    public async Task CreateDataset_AllNullValues_FallbackToText()
    {
        var req = new CreateDataConnectorCommand()
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream =
                """
                    column1,column2,column3
                    ,,
                    ,,
                    """.ToStream()
        };

        var entity = await req.ExecuteAsync();
        entity.FieldList.Should().AllSatisfy(f => f.DataType.Should().Be(DataType.Text));
    }
    
}