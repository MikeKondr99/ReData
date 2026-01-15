using System.Globalization;
using ReData.Query.Core;
using ReData.Query.Core.Value;
using ReData.Query.Impl.Tests.Fixtures;
using ReData.Query.Impl.Tests.Queries;
using ReData.Query.QuerySources;
using ReData.Query.Runners.Value;

namespace ReData.Query.Impl.Tests.Functions.Aggregation;

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
    public async Task Sum_Integer()
    {
        // Arrange
        int?[] arr = [1, 2, 3, 4, 5];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("SUM(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(15));
    }

    [Fact]
    public async Task Sum_Integer_WithNulls()
    {
        // Arrange
        int?[] arr = [1, null, 3, null, 5];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("SUM(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(9)); // 1 + 3 + 5
    }

    [Fact]
    public async Task Sum_Integer_Empty()
    {
        // Arrange
        int?[] arr = [1];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("SUM(x)", ToExpressions(arr), "false");

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(0)); // SUM of empty set should be 0
    }

    [Fact]
    public async Task Sum_Integer_AllNulls()
    {
        // Arrange
        int?[] arr = [1, null, null, null];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("SUM(x)", ToExpressions(arr), "IsNull(x)");

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(0)); // SUM of all nulls should be 0
    }

    [Fact]
    public async Task Sum_Double()
    {
        // Arrange
        double?[] arr = [1.5, 2.5, 3.5];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("SUM(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new NumberValue(7.5)); // 1.5 + 2.5 + 3.5
    }

    [Fact]
    public async Task Sum_Double_WithNulls()
    {
        // Arrange
        double?[] arr = [1.1, null, 2.2, null, 3.3];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("SUM(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new NumberValue(6.6)); // 1.1 + 2.2 + 3.3
    }

    #endregion

    #region Avg

    [Fact]
    public async Task Avg_Integer_Basic()
    {
        // Arrange
        int?[] arr = [1, 2, 3, 4, 5];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("AVG(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new NumberValue(3.0)); // (1+2+3+4+5)/5
    }

    [Fact]
    public async Task Avg_Integer_WithNulls()
    {
        // Arrange
        int?[] arr = [1, null, 3, null, 5];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("AVG(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new NumberValue(3.0)); // (1+3+5)/3
    }

    [Fact]
    public async Task Avg_Integer_Empty()
    {
        // Arrange
        int?[] arr = [1, 2, 3];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("AVG(x)", ToExpressions(arr), "false"); // Filter all out

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, default(NullValue));
    }

    [Fact]
    public async Task Avg_Integer_AllNulls()
    {
        // Arrange
        int?[] arr = [1, 2, null, null];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("AVG(x)", ToExpressions(arr), "IsNull(x)"); // Filter to only nulls

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, default(NullValue));
    }

    [Fact]
    public async Task Avg_Integer_SingleValue()
    {
        // Arrange
        int?[] arr = [5];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("AVG(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new NumberValue(5.0));
    }

    [Fact]
    public async Task Avg_Double_Basic()
    {
        // Arrange
        double?[] arr = [1.5, 2.5, 3.5];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("AVG(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new NumberValue(2.5)); // (1.5+2.5+3.5)/3
    }

    [Fact]
    public async Task Avg_Double_WithNulls()
    {
        // Arrange
        double?[] arr = [1.1, null, 2.2, null, 3.3];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("AVG(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new NumberValue(2.2)); // (1.1+2.2+3.3)/3
    }

    [Fact]
    public async Task Avg_Double_Precision()
    {
        // Arrange
        double?[] arr = [1.0, 2.0, 3.0, 4.0, 5.0];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("AVG(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new NumberValue(3.0));
    }

    [Fact]
    public async Task Avg_DateTime_Basic()
    {
        // Arrange
        DateTime?[] arr =
        [
            new DateTime(2023, 1, 1),
            new DateTime(2023, 1, 2),
            new DateTime(2023, 1, 3)
        ];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("AVG(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        var expectedAvg = new DateTime(2023, 1, 2); // Middle date
        Compare(result, new DateTimeValue(expectedAvg));
    }

    [Fact]
    public async Task Avg_DateTime_WithNulls()
    {
        // Arrange
        DateTime?[] arr =
        [
            new DateTime(2023, 1, 1),
            null,
            new DateTime(2023, 1, 3),
            null
        ];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("AVG(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        var expectedAvg = new DateTime(2023, 1, 2); // (1st + 3rd)/2
        Compare(result, new DateTimeValue(expectedAvg));
    }

    [Fact]
    public async Task Avg_DateTime_Empty()
    {
        // Arrange
        DateTime?[] arr = [new DateTime(2023, 1, 1)];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("AVG(x)", ToExpressions(arr), "false");

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, default(NullValue)); // Or whatever your default is
    }

    [Fact]
    public async Task Avg_DateTime_AllNulls()
    {
        // Arrange
        DateTime?[] arr = [new DateTime(2023, 1, 1), null];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("AVG(x)", ToExpressions(arr), "IsNull(x)");

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, default(NullValue)); // Or whatever your default is
    }

    #endregion

    #region Min

    [Fact]
    public async Task Min_Integer_Basic()
    {
        // Arrange
        int?[] arr = [5, 3, 8, 1, 4];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("MIN(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(1));
    }

    [Fact]
    public async Task Min_Integer_WithNulls()
    {
        // Arrange
        int?[] arr = [5, null, 3, null, 1];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("MIN(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(1));
    }

    [Fact]
    public async Task Min_Integer_Empty()
    {
        // Arrange
        int?[] arr = [1, 2, 3];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("MIN(x)", ToExpressions(arr), "false");

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, default(NullValue));
    }

    [Fact]
    public async Task Min_Integer_AllNulls()
    {
        // Arrange
        int?[] arr = [10, null, null, null];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("MIN(x)", ToExpressions(arr), "x.IsNull()");

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, default(NullValue));
    }

    [Fact]
    public async Task Min_Double_Basic()
    {
        // Arrange
        double?[] arr = [5.5, 3.3, 1.1, 4.4];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("MIN(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new NumberValue(1.1));
    }

    [Fact]
    public async Task Min_DateTime_Basic()
    {
        // Arrange
        DateTime?[] arr =
        [
            new DateTime(2023, 1, 3),
            new DateTime(2023, 1, 1),
            new DateTime(2023, 1, 2)
        ];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("MIN(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new DateTimeValue(new DateTime(2023, 1, 1)));
    }

    [Fact]
    public async Task Min_String_Basic()
    {
        // Arrange
        string?[] arr = ["banana", "apple", "cherry"];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("MIN(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new TextValue("apple"));
    }

    [Fact]
    public async Task Min_String_WithNulls()
    {
        // Arrange
        string?[] arr = ["banana", null, "apple"];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("MIN(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new TextValue("apple"));
    }

    #endregion

    #region Max

    [Fact]
    public async Task Max_Integer_Basic()
    {
        // Arrange
        int?[] arr = [5, 3, 8, 1, 4];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("MAX(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(8));
    }

    [Fact]
    public async Task Max_Integer_WithNulls()
    {
        // Arrange
        int?[] arr = [5, null, 3, null, 8];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("MAX(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(8));
    }

    [Fact]
    public async Task Max_Integer_Empty()
    {
        // Arrange
        int?[] arr = [1, 2, 3];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("MAX(x)", ToExpressions(arr), "false");

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, default(NullValue));
    }

    [Fact]
    public async Task Max_Double_Basic()
    {
        // Arrange
        double?[] arr = [5.5, 3.3, 8.8, 1.1];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("MAX(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new NumberValue(8.8));
    }

    [Fact]
    public async Task Max_DateTime_Basic()
    {
        // Arrange
        DateTime?[] arr =
        [
            new DateTime(2023, 1, 1),
            new DateTime(2023, 1, 3),
            new DateTime(2023, 1, 2)
        ];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("MAX(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new DateTimeValue(new DateTime(2023, 1, 3)));
    }

    [Fact]
    public async Task Max_String_Basic()
    {
        // Arrange
        string?[] arr = ["banana", "apple", "cherry"];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("MAX(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new TextValue("cherry"));
    }

    [Fact]
    public async Task Max_String_WithNulls()
    {
        // Arrange
        string?[] arr = ["banana", null, "apple"];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("MAX(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new TextValue("banana"));
    }

    [Fact]
    public async Task Max_String_AllNulls()
    {
        // Arrange
        string?[] arr = ["text", null, null, null];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("MAX(x)", ToExpressions(arr), "x.IsNull()");

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, default(NullValue));
    }

    #endregion

    #region Count

    [Fact]
    public async Task Count_AllRows_Basic()
    {
        // Arrange
        int?[] arr = [1, 2, 3, 4, 5];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("COUNT()", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(5));
    }

    [Fact]
    public async Task Count_AllRows_WithNulls()
    {
        // Arrange
        int?[] arr = [1, null, 3, null, 5];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("COUNT()", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(5)); // Counts all rows regardless of nulls
    }

    [Fact]
    public async Task Count_AllRows_Empty()
    {
        // Arrange
        int?[] arr = [1, 2, 3];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("COUNT()", ToExpressions(arr), "false");

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(0));
    }

    [Fact]
    public async Task Count_AllRows_AllNulls()
    {
        // Arrange
        int?[] arr = [null, null, null];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("COUNT()", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(3)); // Counts rows, not values
    }

    #endregion

    #region Count Column

    [Fact]
    public async Task Count_Column_Integer_Basic()
    {
        // Arrange
        int?[] arr = [1, 2, 3, 4, 5];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("COUNT(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(5));
    }

    [Fact]
    public async Task Count_Column_Integer_WithNulls()
    {
        // Arrange
        int?[] arr = [1, null, 3, null, 5];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("COUNT(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(3)); // Only counts non-null values
    }

    [Fact]
    public async Task Count_Column_Integer_Empty()
    {
        // Arrange
        int?[] arr = [1, 2, 3];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("COUNT(x)", ToExpressions(arr), "false");

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(0));
    }

    [Fact]
    public async Task Count_Column_Integer_AllNulls()
    {
        // Arrange
        int?[] arr = [12, null, null, null];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("COUNT(x)", ToExpressions(arr), "x.IsNull()");

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(0)); // No non-null values
    }

    [Fact]
    public async Task Count_Column_Double_Basic()
    {
        // Arrange
        double?[] arr = [1.1, 2.2, 3.3];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("COUNT(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(3));
    }

    [Fact]
    public async Task Count_Column_Double_WithNulls()
    {
        // Arrange
        double?[] arr = [1.1, null, 3.3, null];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("COUNT(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(2)); // Only counts non-null values
    }

    [Fact]
    public async Task Count_Column_DateTime_Basic()
    {
        // Arrange
        DateTime?[] arr =
        [
            new DateTime(2023, 1, 1),
            new DateTime(2023, 1, 2),
            new DateTime(2023, 1, 3)
        ];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("COUNT(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(3));
    }

    [Fact]
    public async Task Count_Column_DateTime_WithNulls()
    {
        // Arrange
        DateTime?[] arr =
        [
            new DateTime(2023, 1, 1),
            null,
            null,
            new DateTime(2023, 1, 3)
        ];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("COUNT(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(2)); // Only counts non-null values
    }

    [Fact]
    public async Task Count_Column_String_Basic()
    {
        // Arrange
        string?[] arr = ["a", "b", "c"];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("COUNT(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(3));
    }

    [Fact]
    public async Task Count_Column_String_WithNulls()
    {
        // Arrange
        string?[] arr = ["a", null, "c", null];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("COUNT(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(2)); // Only counts non-null values
    }

    [Fact]
    public async Task Count_Column_String_EmptyStrings()
    {
        // Arrange
        string?[] arr = ["", "", ""]; // Empty strings are still values
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("COUNT(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(3)); // Empty string is still a value
    }

    #endregion

    #region Count Dictinct

    [Fact]
    public async Task CountDistinct_Integer_Basic()
    {
        // Arrange
        int?[] arr = [1, 2, 3, 2, 1];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("COUNT_DISTINCT(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(3)); // Distinct values: 1, 2, 3
    }

    [Fact]
    public async Task CountDistinct_Integer_WithNulls()
    {
        // Arrange
        int?[] arr = [1, null, 2, null, 1];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("COUNT_DISTINCT(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(2)); // Distinct non-null values: 1, 2
    }

    [Fact]
    public async Task CountDistinct_Integer_Empty()
    {
        // Arrange
        int?[] arr = [1, 2, 3];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("COUNT_DISTINCT(x)", ToExpressions(arr), "false");

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(0));
    }

    [Fact]
    public async Task CountDistinct_Integer_AllNulls()
    {
        // Arrange
        int?[] arr = [1, null, null, null];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("COUNT_DISTINCT(x)", ToExpressions(arr), "x.IsNull()");

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(0)); // Nulls are not counted in distinct
    }

    [Fact]
    public async Task CountDistinct_Integer_AllSame()
    {
        // Arrange
        int?[] arr = [5, 5, 5, 5];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("COUNT_DISTINCT(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(1));
    }

    [Fact]
    public async Task CountDistinct_Double_Basic()
    {
        // Arrange
        double?[] arr = [1.1, 2.2, 1.1, 3.3];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("COUNT_DISTINCT(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(3)); // Distinct: 1.1, 2.2, 3.3
    }

    [Fact]
    public async Task CountDistinct_Double_WithNulls()
    {
        // Arrange
        double?[] arr = [1.1, null, 2.2, null];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("COUNT_DISTINCT(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(2)); // Distinct non-null: 1.1, 2.2
    }

    [Fact]
    public async Task CountDistinct_DateTime_Basic()
    {
        // Arrange
        DateTime?[] arr =
        [
            new DateTime(2023, 1, 1),
            new DateTime(2023, 1, 2),
            new DateTime(2023, 1, 1)
        ];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("COUNT_DISTINCT(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(2)); // Distinct dates
    }

    [Fact]
    public async Task CountDistinct_DateTime_WithNulls()
    {
        // Arrange
        DateTime?[] arr =
        [
            new DateTime(2023, 1, 1),
            null,
            new DateTime(2023, 1, 1)
        ];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("COUNT_DISTINCT(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(1)); // Only one distinct date
    }

    [Fact]
    public async Task CountDistinct_Text_Basic()
    {
        // Arrange
        string?[] arr = ["apple", "banana", "apple"];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("COUNT_DISTINCT(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(2)); // Distinct: "apple", "banana"
    }

    [Fact]
    public async Task CountDistinct_Text_WithNulls()
    {
        // Arrange
        string?[] arr = ["apple", null, "banana", null];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("COUNT_DISTINCT(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(2)); // Distinct non-null: "apple", "banana"
    }

    [Fact]
    public async Task CountDistinct_Text_CaseSensitive()
    {
        // Arrange
        string?[] arr = ["Apple", "apple", "APPLE"];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("COUNT_DISTINCT(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        // Behavior depends on database collation
        Compare(result, new IntegerValue(3)); // Assuming case-sensitive comparison
    }

    [Fact]
    public async Task CountDistinct_Text_EmptyStrings()
    {
        // Arrange
        string?[] arr = ["", "", "a"];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("COUNT_DISTINCT(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(2)); // Distinct: "" and "a"
    }

    #endregion

    #region Only

    [Fact]
    public async Task Only_SingleValue_Integer()
    {
        // Arrange
        int?[] arr = [5];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("ONLY(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(5));
    }

    [Fact]
    public async Task Only_AllSameValues_Integer()
    {
        // Arrange
        int?[] arr = [5, 5, 5];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("ONLY(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(5));
    }

    [Fact]
    public async Task Only_MultipleValues_Integer()
    {
        // Arrange
        int?[] arr = [5, 6];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("ONLY(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, default(NullValue));
    }

    [Fact]
    public async Task Only_WithNulls_Integer()
    {
        // Arrange
        int?[] arr = [5, null, null];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("ONLY(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new IntegerValue(5)); // Only considers non-null values
    }

    [Fact]
    public async Task Only_EmptySet_Integer()
    {
        // Arrange
        int?[] arr = [1, 2, 3];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("ONLY(x)", ToExpressions(arr), "false");

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, default(NullValue));
    }

    [Fact]
    public async Task Only_AllNulls_Integer()
    {
        // Arrange
        int?[] arr = [1, null, null];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("ONLY(x)", ToExpressions(arr), "x.IsNull()");

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, default(NullValue)); // No non-null values
    }

    [Fact]
    public async Task Only_SingleValue_Double()
    {
        // Arrange
        double?[] arr = [3.14];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("ONLY(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new NumberValue(3.14));
    }

    [Fact]
    public async Task Only_MultipleValues_Double()
    {
        // Arrange
        double?[] arr = [3.14, 2.71];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("ONLY(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, default(NullValue));
    }

    [Fact]
    public async Task Only_SingleValue_DateTime()
    {
        // Arrange
        DateTime?[] arr = [new DateTime(2023, 1, 1)];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("ONLY(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new DateTimeValue(new DateTime(2023, 1, 1)));
    }

    [Fact]
    public async Task Only_MultipleValues_DateTime()
    {
        // Arrange
        DateTime?[] arr =
        [
            new DateTime(2023, 1, 1),
            new DateTime(2023, 1, 2)
        ];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("ONLY(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, default(NullValue));
    }

    [Fact]
    public async Task Only_SingleValue_Text()
    {
        // Arrange
        string?[] arr = ["unique"];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("ONLY(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new TextValue("unique"));
    }

    [Fact]
    public async Task Only_MultipleValues_Text()
    {
        // Arrange
        string?[] arr = ["apple", "banana"];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("ONLY(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, default(NullValue));
    }

    [Fact]
    public async Task Only_CaseSensitive_Text()
    {
        // Arrange
        string?[] arr = ["Apple", "apple"];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("ONLY(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, default(NullValue)); // Different values due to case
    }

    [Fact]
    public async Task Only_WithNulls_Text()
    {
        // Arrange
        string?[] arr = ["unique", null, null];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("ONLY(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new TextValue("unique")); // Only one non-null value
    }

    [Fact]
    public async Task Only_EmptyStrings_Text()
    {
        // Arrange
        string?[] arr = ["", ""];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("ONLY(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new TextValue("")); // Empty string is still a distinct value
    }

    #endregion
    
    #region CONCAT(value)
    
    [Fact]
    public async Task Concat_Basic()
    {
        // Arrange
        string[] arr = ["a", "b", "c"];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("CONCAT(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new TextValue("abc")); // Simple concatenation
    }

    [Fact]
    public async Task Concat_WithNulls()
    {
        // Arrange
        string?[] arr = ["a", null, "c"];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("CONCAT(x)", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new TextValue("ac")); // Nulls should be skipped
    }

    [Fact]
    public async Task Concat_Empty()
    {
        // Arrange
        string[] arr = ["1","2"];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("CONCAT(x)", ToExpressions(arr), "false");

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, default(NullValue)); // Empty string for empty input
    }
    
    [Fact]
    public async Task Concat_AllNulls()
    {
        // Arrange
        string?[] arr = ["",null, null, null];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("CONCAT(x)", ToExpressions(arr), "x.IsNull()");

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, default(NullValue)); // Empty string for all nulls
    }

    
    #endregion
    
    #region CONCAT(value, delimiter)
    
    [Fact]
    public async Task Concat_WithDelimiter()
    {
        // Arrange
        string[] arr = ["a", "b", "c"];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("CONCAT(x, ',')", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new TextValue("a,b,c")); // Comma-separated
    }

    [Fact]
    public async Task Concat_WithDelimiterAndNulls()
    {
        // Arrange
        string?[] arr = ["a", null, "c"];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("CONCAT(x, '|')", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new TextValue("a|c")); // Nulls skipped
    }

    [Fact]
    public async Task Concat_WithEmptyDelimiter()
    {
        // Arrange
        string[] arr = ["a", "b", "c"];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("CONCAT(x, '')", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new TextValue("abc")); // Same as basic concat
    }
    
    [Fact]
    public async Task Concat_SingleItem()
    {
        // Arrange
        string[] arr = ["single"];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("CONCAT(x, ',')", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new TextValue("single")); // No delimiter for single item
    }

    [Fact]
    public async Task Concat_Unicode()
    {
        // Arrange
        string[] arr = ["привет", "мир"];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("CONCAT(x, ' ')", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new TextValue("привет мир")); // Unicode handling
    }
    
    #endregion

    #region CONCAT(value, delimiter, sort)

    [Fact]
    public async Task Concat_WithDelimiterAndSort()
    {
        // Arrange
        string?[] arr = ["10", "2", "1",];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("CONCAT(x, '|', Int(x))", ToExpressions(arr));

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, new TextValue("1|2|10")); // Nulls skipped
    }
    
    [Fact]
    public async Task ConcatNulls_WithDelimiterAndSort()
    {
        // Arrange
        string?[] arr = ["10", "2", "1", null, null];
        var runner = await db.GetRunnerAsync();
        var qb = GetInline("CONCAT(x, '|', Reverse(x))", ToExpressions(arr), "x.IsNull()");

        // Act
        var result = await runner.RunQueryAsScalar(qb.Build());

        // Assert
        Compare(result, default(NullValue));
    }
    

    #endregion
}