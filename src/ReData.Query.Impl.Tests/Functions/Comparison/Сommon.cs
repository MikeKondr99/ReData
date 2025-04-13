using ReData.Query.Impl.QueryBuilders;
using ReData.Query.Impl.Tests.Fixtures;

namespace ReData.Query.Impl.Tests.Functions.Comparison;

public abstract class Сommon(IDatabaseFixture runner) : RawExprTests(runner)
{
     [Theory(DisplayName = "Less Than (<)")]
     [InlineData("5 < 10", true)]
     [InlineData("10 < 5", false)]
     [InlineData("1.5 < 1.3", false)]
     [InlineData("1.3 < 1.5", true)]
     [InlineData("-4.0 < -2.0", true)]
     // Edge cases with null TODO: IsNull(Bool)
     // [InlineData("IsNull(null < 5)", true)] // Edge case: null comparison
     // [InlineData("IsNull(5 < null)", true)] // Edge case: null comparison
     // [InlineData("IsNull(null < null)", true)] // Edge case: null comparison
     public Task LessThan(string expr, object? expected) => Test(expr, expected);
     
     
     [Theory(DisplayName = "Greater Than (>)")]
     [InlineData("5 > 10", false)]
     [InlineData("10 > 5", true)]
     [InlineData("1.5 > 1.3", true)]
     [InlineData("1.3 > 1.5", false)]
     [InlineData("-2.0 > -4.0", true)]
     // Edge cases with null TODO: IsNull(Bool)
     // [InlineData("IsNull(null > 5)", true)] // Edge case: null comparison
     // [InlineData("IsNull(5 > null)", true)] // Edge case: null comparison
     // [InlineData("IsNull(null > null)", true)] // Edge case: null comparison
     public Task GreaterThan(string expr, object? expected) => Test(expr, expected);
     
     
     [Theory(DisplayName = "Less Than or Equal To (<=)")]
     [InlineData("5 <= 10", true)]
     [InlineData("10 <= 5", false)]
     [InlineData("1.5 <= 1.3", false)]
     [InlineData("1.3 <= 1.5", true)]
     [InlineData("1.5 <= 1.5", true)] // Equal values
     [InlineData("-4.0 <= -2.0", true)]
     // Edge cases with null TODO: IsNull(Bool)
     // [InlineData("IsNull(null <= 5)", true)] // Edge case: null comparison
     // [InlineData("IsNull(5 <= null)", true)] // Edge case: null comparison
     // [InlineData("IsNull(null <= null)", true)] // Edge case: null comparison
     public Task LessThanOrEqual(string expr, object? expected) => Test(expr, expected);
     
     
     [Theory(DisplayName = "Greater Than or Equal To (>=)")]
     [InlineData("5 >= 10", false)]
     [InlineData("10 >= 5", true)]
     [InlineData("1.5 >= 1.3", true)]
     [InlineData("1.3 >= 1.5", false)]
     [InlineData("1.5 >= 1.5", true)] // Equal values
     [InlineData("-2.0 >= -4.0", true)]
     // Edge cases with null TODO: IsNull(Bool)
     // [InlineData("IsNull(null >= 5)", true)] // Edge case: null comparison
     // [InlineData("IsNull(5 >= null)", true)] // Edge case: null comparison
     // [InlineData("IsNull(null >= null)", true)] // Edge case: null comparison
     public Task GreaterThanOrEqual(string expr, object? expected) => Test(expr, expected);
     
     
     [Theory(DisplayName = "Equal To (=)")]
     [InlineData("5 = 5", true)]
     [InlineData("5 = 10", false)]
     [InlineData("1.5 = 1.5", true)]
     [InlineData("1.5 = 1.3", false)]
     [InlineData("-4.0 = -4.0", true)]
     // Edge cases with null TODO: IsNull(Bool)
     // [InlineData("IsNull(null = 5)", true)] // Edge case: null comparison
     // [InlineData("IsNull(5 = null)", true)] // Edge case: null comparison
     // [InlineData("IsNull(null = null)", true)] // Edge case: null comparison
     public Task EqualTo(string expr, object? expected) => Test(expr, expected);
     
     
     [Theory(DisplayName = "Not Equal To (!=)")]
     [InlineData("5 != 10", true)]
     [InlineData("5 != 5", false)]
     [InlineData("1.5 != 1.3", true)]
     [InlineData("1.5 != 1.5", false)]
     [InlineData("-4.0 != -2.0", true)]
     // Edge cases with null TODO: IsNull(Bool)
     // [InlineData("IsNull(null != 5)", true)] // Edge case: null comparison
     // [InlineData("IsNull(5 != null)", true)] // Edge case: null comparison
     // [InlineData("IsNull(null != null)", true)] // Edge case: null comparison
     public Task NotEqualTo(string expr, object? expected) => Test(expr, expected);
     
     
     [Theory(DisplayName = "IsNull")]
     [InlineData("IsNull(null)", true)]
     [InlineData("IsNull(5)", false)]
     [InlineData("IsNull(1.5)", false)]
     [InlineData("IsNull(-4.0)", false)]
     [InlineData("null.IsNull()", true)]
     [InlineData("5.IsNull()", false)]
     [InlineData("1.5.IsNull()", false)]
     [InlineData("(-4.0).IsNull()", false)]
     [InlineData("IsNull(null + 5)", true)] // Edge case: null in expression
     [InlineData("IsNull(5 + null)", true)] // Edge case: null in expression
     [InlineData("IsNull(5 + null).Type()", "Bool")] // returns always not null
     public Task IsNullTest(string expr, object? expected) => Test(expr, expected);
     
     
     [Theory(DisplayName = "Between Function")]
     // Integer tests
     [InlineData("5.Between(1, 10)", true)] // 5 is between 1 and 10
     [InlineData("15.Between(1, 10)", false)] // 15 is not between 1 and 10
     [InlineData("Between(5, 1, 10)", true)] // 5 is between 1 and 10 (alternate syntax)
     [InlineData("Between(15, 1, 10)", false)] // 15 is not between 1 and 10 (alternate syntax)

     // Decimal tests
     [InlineData("1.5.Between(1.0, 2.0)", true)] // 1.5 is between 1.0 and 2.0
     [InlineData("0.9.Between(1.0, 2.0)", false)] // 0.9 is not between 1.0 and 2.0
     [InlineData("Between(1.5, 1.0, 2.0)", true)] // 1.5 is between 1.0 and 2.0 (alternate syntax)
     [InlineData("Between(0.9, 1.0, 2.0)", false)] // 0.9 is not between 1.0 and 2.0 (alternate syntax)

     // Edge cases with null TODO: IsNull(Bool)
     // [InlineData("IsNull(null.Between(1, 10))",true)] // null is not comparable
     // [InlineData("IsNull(5.Between(null, 10))", true)] // null in lower bound
     // [InlineData("IsNull(5.Between(1, null))", true)] // null in upper bound
     // [InlineData("IsNull(Between(null, 1, 10))", true)] // null as value (alternate syntax)
     // [InlineData("IsNull(Between(5, null, 10))", true)] // null in lower bound (alternate syntax)
     // [InlineData("IsNull(Between(5, 1, null))", true)] // null in upper bound (alternate syntax)
     // [InlineData("IsNull(Between(null, null, null))", true)] // all nulls

     // Inclusive bounds
     [InlineData("1.Between(1, 10)", true)] // 1 is equal to lower bound (inclusive)
     [InlineData("10.Between(1, 10)", true)] // 10 is equal to upper bound (inclusive)
     [InlineData("0.Between(1, 10)", false)] // 0 is outside lower bound
     [InlineData("11.Between(1, 10)", false)] // 11 is outside upper bound

     // Mixed types (int and decimal)
     [InlineData("5.Between(1.5, 10.5)", true)] // 5 is between 1.5 and 10.5
     [InlineData("Between(5.5, 1, 10)", true)] // 5.5 is between 1 and 10
     public Task BetweenFunction(string expr, object? expected) => Test(expr, expected);
}