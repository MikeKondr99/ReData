using System.Globalization;
using ReData.Query.Core;
using ReData.Query.Core.Value;
using ReData.Query.Impl.Tests.Fixtures;
using ReData.Query.Impl.Tests.Queries;
using ReData.Query.QuerySources;

namespace ReData.Query.Impl.Tests.Functions.Aggregation;


#pragma warning disable CS9113
public abstract class Сommon(IDatabaseFixture db, ITestAssets assets) : ExprTests(db)
{
#pragma warning disable CA1822
    private QueryBuilder GetInline(string expr, string[] data, string? where = null)
#pragma warning restore CA1822
    {
        return null!;
        // var qb = new InlineQuerySource("x", data, assets.DatabaseType).ToQueryBuilder();
        // if (where is not null)
        // {
        //     qb = qb.Where(where).Expect("Where must be valid");
        // }
        //
        // qb = qb.Select(new()
        // {
        //     ["test"] = expr
        // }).Expect("Query must be valid");
        // return qb;
    }

    private static string[] ToExpressions(int?[] values)
    {
        return values.Select(v => v.HasValue ? $"{v.Value}" : "null").ToArray();
    }

    private static string[] ToExpressions(double?[] values)
    {
        return values.Select(v => v.HasValue ? v.Value.ToString(CultureInfo.InvariantCulture) : "null").ToArray();
    }

    private static string[] ToExpressions(string?[] values)
    {
        return values.Select(v => v is not null ? $"'{v}'" : "null").ToArray();
    }

    private static string[] ToExpressions(System.DateTime?[] values)
    {
        return values.Select(v => v is not null ? $"Date('{v.Value:yyyy-M-dd HH:mm:ss}')" : "null").ToArray();
    }

    #region Sum

    [Fact]
    public async Task SumInteger()
    {
        // Arrange
        int?[] arr = [1, 2, 3, 4, 5];
        var qb = GetInline("SUM(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(15));
    }

    [Fact]
    public async Task SumInteger_WithNulls()
    {
        // Arrange
        int?[] arr = [1, null, 3, null, 5];
        var qb = GetInline("SUM(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(9)); // 1 + 3 + 5
    }

    [Fact]
    public async Task SumIntegerEmpty()
    {
        // Arrange
        int?[] arr = [1];
        var qb = GetInline("SUM(x)", ToExpressions(arr), "false");

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(0)); // SUM of empty set should be 0
    }

    [Fact]
    public async Task SumIntegerAllNulls()
    {
        // Arrange
        int?[] arr = [1, null, null, null];
        var qb = GetInline("SUM(x)", ToExpressions(arr), "IsNull(x)");

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(0)); // SUM of all nulls should be 0
    }

    [Fact]
    public async Task SumDouble()
    {
        // Arrange
        double?[] arr = [1.5, 2.5, 3.5];
        var qb = GetInline("SUM(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new NumberValue(7.5)); // 1.5 + 2.5 + 3.5
    }

    [Fact]
    public async Task SumDouble_WithNulls()
    {
        // Arrange
        double?[] arr = [1.1, null, 2.2, null, 3.3];
        var qb = GetInline("SUM(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new NumberValue(6.6)); // 1.1 + 2.2 + 3.3
    }

    #endregion

    #region Avg

    [Fact]
    public async Task AvgIntegerBasic()
    {
        // Arrange
        int?[] arr = [1, 2, 3, 4, 5];
        var qb = GetInline("AVG(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new NumberValue(3.0)); // (1+2+3+4+5)/5
    }

    [Fact]
    public async Task AvgIntegerWithNulls()
    {
        // Arrange
        int?[] arr = [1, null, 3, null, 5];
        var qb = GetInline("AVG(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new NumberValue(3.0)); // (1+3+5)/3
    }

    [Fact]
    public async Task AvgIntegerEmpty()
    {
        // Arrange
        int?[] arr = [1, 2, 3];
        var qb = GetInline("AVG(x)", ToExpressions(arr), "false"); // Filter all out

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, default(NullValue));
    }

    [Fact]
    public async Task AvgIntegerAllNulls()
    {
        // Arrange
        int?[] arr = [1, 2, null, null];
        var qb = GetInline("AVG(x)", ToExpressions(arr), "IsNull(x)"); // Filter to only nulls

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, default(NullValue));
    }

    [Fact]
    public async Task AvgIntegerSingleValue()
    {
        // Arrange
        int?[] arr = [5];
        var qb = GetInline("AVG(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new NumberValue(5.0));
    }

    [Fact]
    public async Task AvgDoubleBasic()
    {
        // Arrange
        double?[] arr = [1.5, 2.5, 3.5];
        var qb = GetInline("AVG(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new NumberValue(2.5)); // (1.5+2.5+3.5)/3
    }

    [Fact]
    public async Task AvgDoubleWithNulls()
    {
        // Arrange
        double?[] arr = [1.1, null, 2.2, null, 3.3];
        var qb = GetInline("AVG(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new NumberValue(2.2)); // (1.1+2.2+3.3)/3
    }

    [Fact]
    public async Task AvgDoublePrecision()
    {
        // Arrange
        double?[] arr = [1.0, 2.0, 3.0, 4.0, 5.0];
        var qb = GetInline("AVG(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new NumberValue(3.0));
    }

    [Fact]
    public async Task AvgDateTimeBasic()
    {
        // Arrange
        DateTime?[] arr =
        [
            new DateTime(2023, 1, 1),
            new DateTime(2023, 1, 2),
            new DateTime(2023, 1, 3)
        ];
        var qb = GetInline("AVG(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        var expectedAvg = new DateTime(2023, 1, 2); // Middle date
        Compare(result, new DateTimeValue(expectedAvg));
    }

    [Fact]
    public async Task AvgDateTimeWithNulls()
    {
        // Arrange
        DateTime?[] arr =
        [
            new DateTime(2023, 1, 1),
            null,
            new DateTime(2023, 1, 3),
            null
        ];
        var qb = GetInline("AVG(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        var expectedAvg = new DateTime(2023, 1, 2); // (1st + 3rd)/2
        Compare(result, new DateTimeValue(expectedAvg));
    }

    [Fact]
    public async Task AvgDateTimeEmpty()
    {
        // Arrange
        DateTime?[] arr = [new DateTime(2023, 1, 1)];
        var qb = GetInline("AVG(x)", ToExpressions(arr), "false");

        // Act
        var result = await GetScalarAsync(qb);
        ;

        // Assert
        Compare(result, default(NullValue)); // Or whatever your default is
    }

    [Fact]
    public async Task AvgDateTimeAllNulls()
    {
        // Arrange
        DateTime?[] arr = [new DateTime(2023, 1, 1), null];
        var qb = GetInline("AVG(x)", ToExpressions(arr), "IsNull(x)");

        // Act
        var result = await GetScalarAsync(qb);
        ;

        // Assert
        Compare(result, default(NullValue)); // Or whatever your default is
    }

    #endregion

    #region Min

    [Fact]
    public async Task MinIntegerBasic()
    {
        // Arrange
        int?[] arr = [5, 3, 8, 1, 4];
        var qb = GetInline("MIN(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);
        ;

        // Assert
        Compare(result, new IntegerValue(1));
    }

    [Fact]
    public async Task MinIntegerWithNulls()
    {
        // Arrange
        int?[] arr = [5, null, 3, null, 1];
        var qb = GetInline("MIN(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(1));
    }

    [Fact]
    public async Task MinIntegerEmpty()
    {
        // Arrange
        int?[] arr = [1, 2, 3];
        var qb = GetInline("MIN(x)", ToExpressions(arr), "false");

        // Act
        var result = await GetScalarAsync(qb);
        ;

        // Assert
        Compare(result, default(NullValue));
    }

    [Fact]
    public async Task MinIntegerAllNulls()
    {
        // Arrange
        int?[] arr = [10, null, null, null];
        var qb = GetInline("MIN(x)", ToExpressions(arr), "x.IsNull()");

        // Act
        var result = await GetScalarAsync(qb);
        ;

        // Assert
        Compare(result, default(NullValue));
    }

    [Fact]
    public async Task MinDoubleBasic()
    {
        // Arrange
        double?[] arr = [5.5, 3.3, 1.1, 4.4];
        var qb = GetInline("MIN(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);
        ;

        // Assert
        Compare(result, new NumberValue(1.1));
    }

    [Fact]
    public async Task MinDateTimeBasic()
    {
        // Arrange
        DateTime?[] arr =
        [
            new DateTime(2023, 1, 3),
            new DateTime(2023, 1, 1),
            new DateTime(2023, 1, 2)
        ];
        var qb = GetInline("MIN(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new DateTimeValue(new DateTime(2023, 1, 1)));
    }

    [Fact]
    public async Task MinStringBasic()
    {
        // Arrange
        string?[] arr = ["banana", "apple", "cherry"];
        var qb = GetInline("MIN(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new TextValue("apple"));
    }

    [Fact]
    public async Task MinStringWithNulls()
    {
        // Arrange
        string?[] arr = ["banana", null, "apple"];
        var qb = GetInline("MIN(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new TextValue("apple"));
    }

    #endregion

    #region Max

    [Fact]
    public async Task MaxIntegerBasic()
    {
        // Arrange
        int?[] arr = [5, 3, 8, 1, 4];
        var qb = GetInline("MAX(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(8));
    }

    [Fact]
    public async Task MaxIntegerWithNulls()
    {
        // Arrange
        int?[] arr = [5, null, 3, null, 8];
        var qb = GetInline("MAX(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(8));
    }

    [Fact]
    public async Task MaxIntegerEmpty()
    {
        // Arrange
        int?[] arr = [1, 2, 3];
        var qb = GetInline("MAX(x)", ToExpressions(arr), "false");

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, default(NullValue));
    }

    [Fact]
    public async Task MaxDoubleBasic()
    {
        // Arrange
        double?[] arr = [5.5, 3.3, 8.8, 1.1];
        var qb = GetInline("MAX(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new NumberValue(8.8));
    }

    [Fact]
    public async Task MaxDateTimeBasic()
    {
        // Arrange
        DateTime?[] arr =
        [
            new DateTime(2023, 1, 1),
            new DateTime(2023, 1, 3),
            new DateTime(2023, 1, 2)
        ];
        var qb = GetInline("MAX(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new DateTimeValue(new DateTime(2023, 1, 3)));
    }

    [Fact]
    public async Task MaxStringBasic()
    {
        // Arrange
        string?[] arr = ["banana", "apple", "cherry"];
        var qb = GetInline("MAX(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new TextValue("cherry"));
    }

    [Fact]
    public async Task MaxStringWithNulls()
    {
        // Arrange
        string?[] arr = ["banana", null, "apple"];
        var qb = GetInline("MAX(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new TextValue("banana"));
    }

    [Fact]
    public async Task MaxStringAllNulls()
    {
        // Arrange
        string?[] arr = ["text", null, null, null];
        var qb = GetInline("MAX(x)", ToExpressions(arr), "x.IsNull()");

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, default(NullValue));
    }

    #endregion

    #region Count

    [Fact]
    public async Task CountAllRowsBasic()
    {
        // Arrange
        int?[] arr = [1, 2, 3, 4, 5];
        var qb = GetInline("COUNT()", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(5));
    }

    [Fact]
    public async Task CountAllRowsWithNulls()
    {
        // Arrange
        int?[] arr = [1, null, 3, null, 5];
        var qb = GetInline("COUNT()", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(5)); // Counts all rows regardless of nulls
    }

    [Fact]
    public async Task CountAllRowsEmpty()
    {
        // Arrange
        int?[] arr = [1, 2, 3];
        var qb = GetInline("COUNT()", ToExpressions(arr), "false");

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(0));
    }

    [Fact]
    public async Task CountAllRowsAllNulls()
    {
        // Arrange
        int?[] arr = [null, null, null];
        var qb = GetInline("COUNT()", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(3)); // Counts rows, not values
    }

    #endregion

    #region Count Column

    [Fact]
    public async Task CountColumnIntegerBasic()
    {
        // Arrange
        int?[] arr = [1, 2, 3, 4, 5];
        var qb = GetInline("COUNT(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(5));
    }

    [Fact]
    public async Task CountColumnIntegerWithNulls()
    {
        // Arrange
        int?[] arr = [1, null, 3, null, 5];
        var qb = GetInline("COUNT(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(3)); // Only counts non-null values
    }

    [Fact]
    public async Task CountColumnIntegerEmpty()
    {
        // Arrange
        int?[] arr = [1, 2, 3];
        var qb = GetInline("COUNT(x)", ToExpressions(arr), "false");

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(0));
    }

    [Fact]
    public async Task CountColumnIntegerAllNulls()
    {
        // Arrange
        int?[] arr = [12, null, null, null];
        var qb = GetInline("COUNT(x)", ToExpressions(arr), "x.IsNull()");

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(0)); // No non-null values
    }

    [Fact]
    public async Task CountColumnDoubleBasic()
    {
        // Arrange
        double?[] arr = [1.1, 2.2, 3.3];
        var qb = GetInline("COUNT(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(3));
    }

    [Fact]
    public async Task CountColumnDoubleWithNulls()
    {
        // Arrange
        double?[] arr = [1.1, null, 3.3, null];
        var qb = GetInline("COUNT(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(2)); // Only counts non-null values
    }

    [Fact]
    public async Task CountColumnDateTimeBasic()
    {
        // Arrange
        DateTime?[] arr =
        [
            new DateTime(2023, 1, 1),
            new DateTime(2023, 1, 2),
            new DateTime(2023, 1, 3)
        ];
        var qb = GetInline("COUNT(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(3));
    }

    [Fact]
    public async Task CountColumnateTimeWithNulls()
    {
        // Arrange
        DateTime?[] arr =
        [
            new DateTime(2023, 1, 1),
            null,
            null,
            new DateTime(2023, 1, 3)
        ];
        var qb = GetInline("COUNT(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(2)); // Only counts non-null values
    }

    [Fact]
    public async Task CountColumnStringBasic()
    {
        // Arrange
        string?[] arr = ["a", "b", "c"];
        var qb = GetInline("COUNT(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(3));
    }

    [Fact]
    public async Task CountColumnStringWithNulls()
    {
        // Arrange
        string?[] arr = ["a", null, "c", null];
        var qb = GetInline("COUNT(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(2)); // Only counts non-null values
    }

    [Fact]
    public async Task CountColumnStringEmptyStrings()
    {
        // Arrange
        string?[] arr = ["", "", ""]; // Empty strings are still values
        var qb = GetInline("COUNT(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(3)); // Empty string is still a value
    }

    #endregion

    #region Count Dictinct

    [Fact]
    public async Task CountDistinctIntegerBasic()
    {
        // Arrange
        int?[] arr = [1, 2, 3, 2, 1];
        var qb = GetInline("COUNT_DISTINCT(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(3)); // Distinct values: 1, 2, 3
    }

    [Fact]
    public async Task CountDistinctIntegerWithNulls()
    {
        // Arrange
        int?[] arr = [1, null, 2, null, 1];
        var qb = GetInline("COUNT_DISTINCT(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(2)); // Distinct non-null values: 1, 2
    }

    [Fact]
    public async Task CountDistinctIntegerEmpty()
    {
        // Arrange
        int?[] arr = [1, 2, 3];
        var qb = GetInline("COUNT_DISTINCT(x)", ToExpressions(arr), "false");

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(0));
    }

    [Fact]
    public async Task CountDistinctIntegerAllNulls()
    {
        // Arrange
        int?[] arr = [1, null, null, null];
        var qb = GetInline("COUNT_DISTINCT(x)", ToExpressions(arr), "x.IsNull()");

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(0)); // Nulls are not counted in distinct
    }

    [Fact]
    public async Task CountDistinctIntegerAllSame()
    {
        // Arrange
        int?[] arr = [5, 5, 5, 5];
        var qb = GetInline("COUNT_DISTINCT(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(1));
    }

    [Fact]
    public async Task CountDistinctDoubleBasic()
    {
        // Arrange
        double?[] arr = [1.1, 2.2, 1.1, 3.3];
        var qb = GetInline("COUNT_DISTINCT(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(3)); // Distinct: 1.1, 2.2, 3.3
    }

    [Fact]
    public async Task CountDistinctDoubleWithNulls()
    {
        // Arrange
        double?[] arr = [1.1, null, 2.2, null];
        var qb = GetInline("COUNT_DISTINCT(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(2)); // Distinct non-null: 1.1, 2.2
    }

    [Fact]
    public async Task CountDistinctDateTimeBasic()
    {
        // Arrange
        DateTime?[] arr =
        [
            new DateTime(2023, 1, 1),
            new DateTime(2023, 1, 2),
            new DateTime(2023, 1, 1)
        ];
        var qb = GetInline("COUNT_DISTINCT(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(2)); // Distinct dates
    }

    [Fact]
    public async Task CountDistinctDateTimeWithNulls()
    {
        // Arrange
        DateTime?[] arr =
        [
            new DateTime(2023, 1, 1),
            null,
            new DateTime(2023, 1, 1)
        ];
        var qb = GetInline("COUNT_DISTINCT(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(1)); // Only one distinct date
    }

    [Fact]
    public async Task CountDistinctTextBasic()
    {
        // Arrange
        string?[] arr = ["apple", "banana", "apple"];
        var qb = GetInline("COUNT_DISTINCT(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(2)); // Distinct: "apple", "banana"
    }

    [Fact]
    public async Task CountDistinctTextWithNulls()
    {
        // Arrange
        string?[] arr = ["apple", null, "banana", null];
        var qb = GetInline("COUNT_DISTINCT(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        // Behavior depends on database collation
        Compare(result, new IntegerValue(3)); // Assuming case-sensitive comparison
    }

    [Fact]
    public async Task CountDistinctTextEmptyStrings()
    {
        // Arrange
        string?[] arr = ["", "", "a"];
        var qb = GetInline("COUNT_DISTINCT(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(2)); // Distinct: "" and "a"
    }

    #endregion

    #region Only

    [Fact]
    public async Task OnlySingleValueInteger()
    {
        // Arrange
        int?[] arr = [5];
        var qb = GetInline("ONLY(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(5));
    }

    [Fact]
    public async Task OnlyAllSameValuesInteger()
    {
        // Arrange
        int?[] arr = [5, 5, 5];
        var qb = GetInline("ONLY(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(5));
    }

    [Fact]
    public async Task OnlyMultipleValuesInteger()
    {
        // Arrange
        int?[] arr = [5, 6];
        var qb = GetInline("ONLY(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, default(NullValue));
    }

    [Fact]
    public async Task OnlyWithNullsInteger()
    {
        // Arrange
        int?[] arr = [5, null, null];
        var qb = GetInline("ONLY(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new IntegerValue(5)); // Only considers non-null values
    }

    [Fact]
    public async Task OnlyEmptySetInteger()
    {
        // Arrange
        int?[] arr = [1, 2, 3];
        var qb = GetInline("ONLY(x)", ToExpressions(arr), "false");

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, default(NullValue));
    }

    [Fact]
    public async Task OnlyAllNullsInteger()
    {
        // Arrange
        int?[] arr = [1, null, null];
        var qb = GetInline("ONLY(x)", ToExpressions(arr), "x.IsNull()");

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, default(NullValue)); // No non-null values
    }

    [Fact]
    public async Task OnlySingleValueDouble()
    {
        // Arrange
        double?[] arr = [3.14];
        var qb = GetInline("ONLY(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new NumberValue(3.14));
    }

    [Fact]
    public async Task OnlyMultipleValuesDouble()
    {
        // Arrange
        double?[] arr = [3.14, 2.71];
        var qb = GetInline("ONLY(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, default(NullValue));
    }

    [Fact]
    public async Task OnlySingleValueDateTime()
    {
        // Arrange
        DateTime?[] arr = [new DateTime(2023, 1, 1)];
        var qb = GetInline("ONLY(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new DateTimeValue(new DateTime(2023, 1, 1)));
    }

    [Fact]
    public async Task OnlyMultipleValuesDateTime()
    {
        // Arrange
        DateTime?[] arr =
        [
            new DateTime(2023, 1, 1),
            new DateTime(2023, 1, 2)
        ];
        var qb = GetInline("ONLY(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, default(NullValue));
    }

    [Fact]
    public async Task OnlySingleValueText()
    {
        // Arrange
        string?[] arr = ["unique"];
        var qb = GetInline("ONLY(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new TextValue("unique"));
    }

    [Fact]
    public async Task OnlyMultipleValuesText()
    {
        // Arrange
        string?[] arr = ["apple", "banana"];
        var qb = GetInline("ONLY(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, default(NullValue));
    }

    [Fact]
    public async Task OnlyCaseSensitiveText()
    {
        // Arrange
        string?[] arr = ["Apple", "apple"];
        var qb = GetInline("ONLY(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, default(NullValue)); // Different values due to case
    }

    [Fact]
    public async Task OnlyWithNullsText()
    {
        // Arrange
        string?[] arr = ["unique", null, null];
        var qb = GetInline("ONLY(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new TextValue("unique")); // Only one non-null value
    }

    [Fact]
    public async Task OnlyEmptyStringsText()
    {
        // Arrange
        string?[] arr = ["", ""];
        var qb = GetInline("ONLY(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new TextValue("")); // Empty string is still a distinct value
    }

    #endregion

    #region CONCAT(value)

    [Fact]
    public async Task ConcatBasic()
    {
        // Arrange
        string[] arr = ["a", "b", "c"];
        var qb = GetInline("CONCAT(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new TextValue("abc")); // Simple concatenation
    }

    [Fact]
    public async Task ConcatWithNulls()
    {
        // Arrange
        string?[] arr = ["a", null, "c"];
        var qb = GetInline("CONCAT(x)", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new TextValue("ac")); // Nulls should be skipped
    }

    [Fact]
    public async Task ConcatEmpty()
    {
        // Arrange
        string[] arr = ["1", "2"];
        var qb = GetInline("CONCAT(x)", ToExpressions(arr), "false");

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, default(NullValue)); // Empty string for empty input
    }

    [Fact]
    public async Task ConcatAllNulls()
    {
        // Arrange
        string?[] arr = ["", null, null, null];
        var qb = GetInline("CONCAT(x)", ToExpressions(arr), "x.IsNull()");

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, default(NullValue)); // Empty string for all nulls
    }

    #endregion

    #region CONCAT(value, delimiter)

    [Fact]
    public async Task ConcatWithDelimiter()
    {
        // Arrange
        string[] arr = ["a", "b", "c"];
        var qb = GetInline("CONCAT(x, ',')", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new TextValue("a,b,c")); // Comma-separated
    }

    [Fact]
    public async Task ConcatWithDelimiterAndNulls()
    {
        // Arrange
        string?[] arr = ["a", null, "c"];
        var qb = GetInline("CONCAT(x, '|')", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new TextValue("a|c")); // Nulls skipped
    }

    [Fact]
    public async Task ConcatWithEmptyDelimiter()
    {
        // Arrange
        string[] arr = ["a", "b", "c"];
        var qb = GetInline("CONCAT(x, '')", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new TextValue("abc")); // Same as basic concat
    }

    [Fact]
    public async Task ConcatSingleItem()
    {
        // Arrange
        string[] arr = ["single"];
        var qb = GetInline("CONCAT(x, ',')", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new TextValue("single")); // No delimiter for single item
    }

    [Fact]
    public async Task ConcatUnicode()
    {
        // Arrange
        string[] arr = ["привет", "мир"];
        var qb = GetInline("CONCAT(x, ' ')", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new TextValue("привет мир")); // Unicode handling
    }

    #endregion

    #region CONCAT(value, delimiter, sort)

    [Fact]
    public async Task ConcatWithDelimiterAndSort()
    {
        // Arrange
        string?[] arr = ["10", "2", "1",];
        var qb = GetInline("CONCAT(x, '|', Int(x))", ToExpressions(arr));

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, new TextValue("1|2|10")); // Nulls skipped
    }

    [Fact]
    public async Task ConcatNullsWithDelimiterAndSort()
    {
        // Arrange
        string?[] arr = ["10", "2", "1", null, null];
        var qb = GetInline("CONCAT(x, '|', Reverse(x))", ToExpressions(arr), "x.IsNull()");

        // Act
        var result = await GetScalarAsync(qb);

        // Assert
        Compare(result, default(NullValue));
    }

    #endregion
}