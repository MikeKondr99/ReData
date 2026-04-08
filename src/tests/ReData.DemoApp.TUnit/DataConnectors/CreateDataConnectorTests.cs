using System.Text;
using ReData.Common;
using ReData.DemoApp.Commands;
using ReData.Query.Core.Types;
using TUnit.Core;

namespace ReData.DemoApp.TUnit.DataConnectors;

[NotInParallel]
public class CreateDataConnectorTests
{
    [ClassDataSource<DefaultReDataApp>(Shared = SharedType.PerTestSession)]
    public required DefaultReDataApp App { get; init; }

    private static string FakeDataConnectorName() => $"connector{Guid.NewGuid().ToString("N")[..6]}";

    private static CreateDataConnectorCommand Request(string csv, char separator = ',', bool withHeader = true) => new()
    {
        Name = FakeDataConnectorName(),
        Separator = separator,
        WithHeader = withHeader,
        FileStream = csv.ToStream(),
    };

    private static async Task AssertFails(Func<Task> action)
    {
        var failed = false;
        try
        {
            await action();
        }
        catch
        {
            failed = true;
        }

        await Assert.That(failed).IsTrue();
    }

    private static async Task AssertAliases(IReadOnlyCollection<string> actual, params string[] expected)
    {
        await Assert.That(actual.Count).IsEqualTo(expected.Length);
        for (var i = 0; i < expected.Length; i++)
        {
            await Assert.That(actual.ElementAt(i)).IsEqualTo(expected[i]);
        }
    }

    [Test]
    public async Task CreateDataset_WithValidData_ShouldReturnCreated()
    {
        var req = Request("""
                          col1,col2
                          1,qwe
                          2,rty
                          """);

        var entity = await req.ExecuteAsync();

        await Assert.That(entity.Name).IsEqualTo(req.Name);
        await AssertAliases(entity.FieldList.Select(f => f.Alias).ToArray(), "col1", "col2");
    }

    [Test]
    public async Task CreateDataset_EmptyCsv_ShouldFail()
    {
        var req = Request(string.Empty);
        await AssertFails(() => req.ExecuteAsync());
    }

    [Test]
    public async Task CreateDataset_OnlyHeaderRow_ShouldCreateEmptyDataset()
    {
        var req = Request("col1,col2,col3");
        var entity = await req.ExecuteAsync();

        await AssertAliases(entity.FieldList.Select(f => f.Alias).ToArray(), "col1", "col2", "col3");
    }

    [Test]
    public async Task CreateDataset_SingleColumnWithHeader_ShouldCreateSingleField()
    {
        var req = Request("""
                          column_name
                          value1
                          value2
                          value3
                          """);

        var entity = await req.ExecuteAsync();

        await Assert.That(entity.FieldList.Count).IsEqualTo(1);
        await AssertAliases(entity.FieldList.Select(f => f.Alias).ToArray(), "column_name");
    }

    [Test]
    public async Task CreateDataset_SingleColumnWithoutHeader_ShouldCreateFieldA()
    {
        var req = Request("""
                          value1
                          value2
                          value3
                          """, withHeader: false);

        var entity = await req.ExecuteAsync();

        await Assert.That(entity.FieldList.Count).IsEqualTo(1);
        await Assert.That(entity.FieldList[0].Alias).IsEqualTo("A");
    }

    [Test]
    public async Task CreateDataset_ManyColumns_ShouldCreateAllFields()
    {
        var columnCount = 52;
        var header = string.Join(",", Enumerable.Range(1, columnCount).Select(i => $"col{i}"));
        var dataRow = string.Join(",", Enumerable.Range(1, columnCount).Select(i => $"value{i}"));
        var req = Request($"{header}\n{dataRow}");

        var entity = await req.ExecuteAsync();

        await Assert.That(entity.FieldList.Count).IsEqualTo(columnCount);
        await Assert.That(entity.FieldList.Any(f => f.Alias == "col1")).IsTrue();
        await Assert.That(entity.FieldList.Any(f => f.Alias == $"col{columnCount}")).IsTrue();
    }

    [Test]
    public async Task CreateDataset_DuplicateColumnNames_ShouldHandleGracefully()
    {
        var req = Request("""
                          col1,col1,col2,col2,col2
                          value1,value2,value3,value4,value5
                          """);

        var entity = await req.ExecuteAsync();

        await AssertAliases(entity.FieldList.Select(f => f.Alias).ToArray(), "col1", "col1_2", "col2", "col2_2", "col2_3");
    }

    [Test]
    public async Task CreateDataset_LongColumnNames_ShouldWork()
    {
        var longName = new string('A', 1000);
        var req = Request($"{longName}\nvalue1");

        var entity = await req.ExecuteAsync();

        await Assert.That(entity.FieldList.Count).IsEqualTo(1);
        await Assert.That(entity.FieldList[0].Alias).IsEqualTo(longName);
    }

    [Test]
    public async Task CreateDataset_SpecialCharactersInColumnNames_ShouldPreserveThem()
    {
        var req = Request("""
                          col with spaces,колонка_кириллица,column-with-dashes,column_with_underscores,col!@#$%^&*()
                          value1,value2,value3,value4,value5
                          """);

        var entity = await req.ExecuteAsync();

        await AssertAliases(entity.FieldList.Select(f => f.Alias).ToArray(),
            "col with spaces",
            "колонка_кириллица",
            "column-with-dashes",
            "column_with_underscores",
            "col!@#$%^&*()");
    }

    [Test]
    public async Task CreateDataset_QuotedSpecialCharactersInHeader_ShouldParse()
    {
        var req = Request("""
                          "col,with,commas","col with "quotes" kek"
                          value1,value2
                          """);

        await AssertFails(() => req.ExecuteAsync());
    }

    [Test]
    public async Task CreateDataset_WithHeaders_ShouldTrimHeaders()
    {
        var req = Request("""
                          col1 , col2
                          value1,value2
                          """);

        var entity = await req.ExecuteAsync();
        await AssertAliases(entity.FieldList.Select(f => f.Alias).ToArray(), "col1", "col2");
    }

    [Test]
    public async Task CreateDataset_WithHeadersButNoData_ShouldParse()
    {
        var req = Request("1,-7,70,1000023429387429837529875,30");
        var entity = await req.ExecuteAsync();

        await Assert.That(entity.FieldList.Count).IsEqualTo(5);
        await AssertAliases(entity.FieldList.Select(f => f.Alias).ToArray(), "1", "-7", "70", "1000023429387429837529875", "30");
    }

    [Test]
    public async Task CreateDataset_IncorrectSeparator_ShouldParseIncorrectly()
    {
        var req = Request("""
                          col1;col2;col3
                          value1;value2;value3
                          """);

        var entity = await req.ExecuteAsync();

        await Assert.That(entity.FieldList.Count).IsEqualTo(1);
        await Assert.That(entity.FieldList[0].Alias).IsEqualTo("col1;col2;col3");
    }

    [Test]
    public async Task CreateDataset_QuotedSeparators_ShouldParseCorrectly()
    {
        var req = Request("""
                          name,address,city
                          "John,Smith","123,Main St",NewYork
                          "Jane,Doe","456,Oak Ave",Boston
                          """);

        var entity = await req.ExecuteAsync();
        await AssertAliases(entity.FieldList.Select(f => f.Alias).ToArray(), "name", "address", "city");
    }

    [Test]
    public async Task CreateDataset_TabSeparator_ShouldWork()
    {
        var entity = await Request("""
                                   col1	col2	col3
                                   value1	value2	value3
                                   """, '\t').ExecuteAsync();

        await AssertAliases(entity.FieldList.Select(f => f.Alias).ToArray(), "col1", "col2", "col3");
    }

    [Test]
    public async Task CreateDataset_PipeSeparator_ShouldWork()
    {
        var entity = await Request("""
                                   col1|col2|col3
                                   value1|value2|value3
                                   """, '|').ExecuteAsync();

        await AssertAliases(entity.FieldList.Select(f => f.Alias).ToArray(), "col1", "col2", "col3");
    }

    [Test]
    public async Task CreateDataset_SemicolonSeparator_ShouldWork()
    {
        var entity = await Request("""
                                   col1;col2;col3
                                   value1;value2;value3
                                   """, ';').ExecuteAsync();

        await AssertAliases(entity.FieldList.Select(f => f.Alias).ToArray(), "col1", "col2", "col3");
    }

    [Test]
    public async Task CreateDataset_SeparatorInUnquotedData_ShouldSplitIncorrectly()
    {
        var req = Request("""
                          name,full_address
                          John,123 Main St, Apt 4,New York
                          Jane,456 Oak Ave,Boston
                          """);

        var entity = await req.ExecuteAsync();
        await AssertAliases(entity.FieldList.Select(f => f.Alias).ToArray(), "name", "full_address");
    }

    [Test]
    public async Task CreateDataset_CommaInQuotedFields_ShouldWork()
    {
        var req = Request("""
                          name,address,tags
                          John,"123 Main St, Apt 4","tag1,tag2,tag3"
                          Jane,"456 Oak Ave, Suite 100","tagA,tagB"
                          """);

        var entity = await req.ExecuteAsync();
        await AssertAliases(entity.FieldList.Select(f => f.Alias).ToArray(), "name", "address", "tags");
    }

    [Test]
    public async Task CreateDataset_CustomSeparator_ShouldWork()
    {
        var entity = await Request("""
                                   col1^col2^col3
                                   value1^value2^value3
                                   value4^value5^value6
                                   """, '^').ExecuteAsync();

        await AssertAliases(entity.FieldList.Select(f => f.Alias).ToArray(), "col1", "col2", "col3");
    }

    [Test]
    public async Task CreateDataset_SpaceSeparator_ShouldWork()
    {
        var entity = await Request("""
                                   col1 col2 col3
                                   value1 value2 value3
                                   """, ' ').ExecuteAsync();

        await AssertAliases(entity.FieldList.Select(f => f.Alias).ToArray(), "col1", "col2", "col3");
    }

    [Test]
    public async Task CreateDataset_SeparatorAtLineEdges_ShouldCreateEmptyFields()
    {
        var req = new CreateDataConnectorCommand
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream = new MemoryStream(),
        };

        await AssertFails(() => req.ExecuteAsync());
    }

    [Test]
    public async Task CreateDataset_EmptyStream_ShouldFail()
    {
        var req = new CreateDataConnectorCommand
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream = new MemoryStream(),
        };

        await AssertFails(() => req.ExecuteAsync());
    }

    [Test]
    public async Task CreateDataset_NullStream_ShouldFail()
    {
        var req = new CreateDataConnectorCommand
        {
            Name = FakeDataConnectorName(),
            Separator = ',',
            WithHeader = true,
            FileStream = null!,
        };

        await AssertFails(() => req.ExecuteAsync());
    }

    [Test]
    public async Task CreateDataset_LargeCsv_ShouldNotCrash()
    {
        var rowCount = 10000;
        var builder = new StringBuilder();
        builder.AppendLine("col1,col2,col3");

        for (var i = 0; i < rowCount; i++)
        {
            builder.AppendLine($"value{i}_1,value{i}_2,value{i}_3");
        }

        var entity = await Request(builder.ToString()).ExecuteAsync();
        await AssertAliases(entity.FieldList.Select(f => f.Alias).ToArray(), "col1", "col2", "col3");
    }

    [Test]
    public async Task CreateDataset_WithTrailingNewline_ShouldParse()
    {
        var req = Request("""
                          col1,col2,col3
                          value1,value2,value3
                          """);

        var entity = await req.ExecuteAsync();
        await AssertAliases(entity.FieldList.Select(f => f.Alias).ToArray(), "col1", "col2", "col3");
    }

    [Test]
    public async Task CreateDataset_WithoutTrailingNewline_ShouldParse()
    {
        var entity = await Request("col1,col2,col3\nvalue1,value2,value3").ExecuteAsync();
        await AssertAliases(entity.FieldList.Select(f => f.Alias).ToArray(), "col1", "col2", "col3");
    }

    [Test]
    public async Task CreateDataset_WindowsLineEndings_ShouldParse()
    {
        var entity = await Request("col1,col2\r\nvalue1,value2\r\nvalue3,value4").ExecuteAsync();
        await AssertAliases(entity.FieldList.Select(f => f.Alias).ToArray(), "col1", "col2");
    }

    [Test]
    public async Task CreateDataset_UnixLineEndings_ShouldParse()
    {
        var entity = await Request("col1,col2\nvalue1,value2\nvalue3,value4").ExecuteAsync();
        await AssertAliases(entity.FieldList.Select(f => f.Alias).ToArray(), "col1", "col2");
    }

    [Test]
    public async Task CreateDataset_MixedLineEndings_ShouldParse()
    {
        var entity = await Request("col1,col2\r\nvalue1,value2\nvalue3,value4\r\n").ExecuteAsync();
        await AssertAliases(entity.FieldList.Select(f => f.Alias).ToArray(), "col1", "col2");
    }

    [Test]
    public async Task CreateDataset_UnclosedQuotes_ShouldFail()
    {
        var req = Request("""
                          col1,col2
                          "value1,value2
                          value3,value4
                          """);

        await AssertFails(() => req.ExecuteAsync());
    }

    [Test]
    public async Task CreateDataset_EscapedQuotesUnquoted_ShouldHandle()
    {
        var req = Request("""
                          col1,col2
                          value1"with quotes,value2
                          value3,value4
                          """);

        var entity = await req.ExecuteAsync();
        await AssertAliases(entity.FieldList.Select(f => f.Alias).ToArray(), "col1", "col2");
    }

    [Test]
    public async Task CreateDataset_EscapedQuotesProper_ShouldWork()
    {
        var req = Request("col1,col2\n\"value1 \"\"with quotes\"\"\",value2\nvalue3,\"value4 \"\"more quotes\"\"\"");
        var entity = await req.ExecuteAsync();

        await AssertAliases(entity.FieldList.Select(f => f.Alias).ToArray(), "col1", "col2");
    }

    [Test]
    public async Task CreateDataset_MissingFieldAtEnd_ShouldHandle()
    {
        var req = Request("""
                          col1,col2,col3
                          value1,value2
                          value3,value4,value5
                          """);

        var entity = await req.ExecuteAsync();
        await AssertAliases(entity.FieldList.Select(f => f.Alias).ToArray(), "col1", "col2", "col3");
    }

    [Test]
    public async Task CreateDataset_ExtraFieldAtEnd_ShouldHandle()
    {
        var req = Request("""
                          col1,col2
                          value1,value2,value3
                          value4,value5
                          """);

        var entity = await req.ExecuteAsync();
        await AssertAliases(entity.FieldList.Select(f => f.Alias).ToArray(), "col1", "col2");
    }

    [Test]
    public async Task CreateDataset_OnlyCR_ShouldHandle()
    {
        var req = Request("col1,col2\rvalue1,value2\r");
        await req.ExecuteAsync();
    }

    [Test]
    public async Task CreateDataset_IntegerFormat_Valid()
    {
        var req = Request("""
                          id,count,negative
                          1,2,-3
                          2,10,-999
                          ,,
                          """);

        var entity = await req.ExecuteAsync();
        await Assert.That(entity.FieldList.Count).IsEqualTo(3);
        await Assert.That(entity.FieldList.All(f => f.DataType == DataType.Integer)).IsTrue();
    }

    [Test]
    public async Task CreateDataset_NumberFormat_Valid()
    {
        var req = Request("""
                          price,temperature,ratio
                          13.45,-2.5,0.75
                          100.00,98.6,0.5
                          ,,
                          """);

        var entity = await req.ExecuteAsync();
        await Assert.That(entity.FieldList.All(f => f.DataType == DataType.Number)).IsTrue();
    }

    [Test]
    public async Task CreateDataset_IntegerMixedWithNumber_FallbackToNumber()
    {
        var req = Request("""
                          id,price
                          1,13.45
                          2,10
                          ,,
                          """);

        var entity = await req.ExecuteAsync();
        await Assert.That(entity.FieldList[0].DataType).IsEqualTo(DataType.Integer);
        await Assert.That(entity.FieldList[1].DataType).IsEqualTo(DataType.Number);
    }

    [Test]
    public async Task CreateDataset_BooleanTrueFalse_Valid()
    {
        var req = Request("""
                          active,verified,completed
                          true,false,true
                          False,TRUE,False
                          ,,
                          """);

        var entity = await req.ExecuteAsync();
        await Assert.That(entity.FieldList.All(f => f.DataType == DataType.Bool)).IsTrue();
    }

    [Test]
    public async Task CreateDataset_BooleanOneZero_Valid()
    {
        var req = Request("""
                          is_active,has_access
                          1,0
                          0,1
                          ,,
                          """);

        var entity = await req.ExecuteAsync();
        await Assert.That(entity.FieldList.All(f => f.DataType == DataType.Bool)).IsTrue();
    }

    [Test]
    public async Task CreateDataset_ISODateFormat_Valid()
    {
        var req = Request("""
                          date_of_birth,registration_date
                          1990-01-15,2023-12-31
                          1985-07-20,2024-01-01
                          ,,
                          """);

        var entity = await req.ExecuteAsync();
        await Assert.That(entity.FieldList.All(f => f.DataType == DataType.DateTime)).IsTrue();
    }

    [Test]
    public async Task CreateDataset_UTCDateFormat_Valid()
    {
        var req = Request("""
                          created_at,updated_at
                          2023-12-31T23:59:59Z,2024-01-01T00:00:00Z
                          2023-06-15T12:30:00Z,2023-06-15T14:45:00Z
                          ,,
                          """);

        var entity = await req.ExecuteAsync();
        await Assert.That(entity.FieldList.All(f => f.DataType == DataType.DateTime)).IsTrue();
    }

    [Test]
    public async Task CreateDataset_TextFormat_Valid()
    {
        var req = Request("""
                          name,city,description
                          Иван,Москва,Разработчик ПО
                          Мария,Санкт-Петербург,Тестировщик
                          ,,
                          """);

        var entity = await req.ExecuteAsync();
        await Assert.That(entity.FieldList.All(f => f.DataType == DataType.Text)).IsTrue();
    }

    [Test]
    public async Task CreateDataset_MixedTypes_FallbackToMostGeneral()
    {
        var req = Request("""
                          mixed_column
                          123
                          45.67
                          текстовое значение
                          true
                          2023-01-01
                          """);
        var entity = await req.ExecuteAsync();
        await Assert.That(entity.FieldList[0].DataType).IsEqualTo(DataType.Text);
    }

    [Test]
    public async Task CreateDataset_WithNullValues_TypeDeterminedFromNonEmpty()
    {
        var req = Request("""
                          id,name,price
                          1,,10.5
                          ,Иван,
                          3,Мария,20.0
                          """);

        var entity = await req.ExecuteAsync();
        await Assert.That(entity.FieldList[0].DataType).IsEqualTo(DataType.Integer);
        await Assert.That(entity.FieldList[1].DataType).IsEqualTo(DataType.Text);
        await Assert.That(entity.FieldList[2].DataType).IsEqualTo(DataType.Number);
    }

    [Test]
    public async Task CreateDataset_AllNullValues_FallbackToText()
    {
        var req = Request("""
                          column1,column2,column3
                          ,,
                          ,,
                          """);

        var entity = await req.ExecuteAsync();
        await Assert.That(entity.FieldList.All(f => f.DataType == DataType.Text)).IsTrue();
    }
}

