using FluentAssertions;
using ReData.Query.Core.Types;

namespace ReData.Query.Impl.Tests.Queries;

public class FailureQueries
{
    [Fact]
    public void CombinationOfAggrAndNonConstShouldFail()
    {
        var qb = new PostgresAssets().UsersQuery.Select(new()
        {
            ["Test"] = "MIN(FirstName) + LastName",
        }).ExpectErr("Должен упасть с ошибкой");
    }
    
    [Fact]
    public void CombinationOfAggrFieldAndNotAggrFieldShouldFail()
    {
        var qb = new PostgresAssets().UsersQuery.Select(new()
        {
            ["Test"] = "MIN(FirstName)",
            ["Test2"] = "LastName",
        }).ExpectErr("Должен упасть с ошибкой");
    }
    
    [Fact]
    public void SelectMustBeNonBoolean()
    {
        var qb = new PostgresAssets().UsersQuery.Select(new()
        {
            ["Test"] = "10 = 10",
        }).ExpectErr("Должен упасть с ошибкой");
    }
    
    [Fact]
    public void SelectMustBeNonNull()
    {
        var qb = new PostgresAssets().UsersQuery.Select(new()
        {
            ["Test"] = "null",
        }).ExpectErr("Должен упасть с ошибкой");
    }
    
    [Fact]
    public void OrderByMustBeNonBoolean()
    {
        var qb = new PostgresAssets().UsersQuery
            .OrderBy([("10 = 10", Order.Type.Asc)])
            .ExpectErr("Должен упасть с ошибкой");
    }
    
    [Fact]
    public void OrderByMustBeNonNull()
    {
        var qb = new PostgresAssets().UsersQuery
            .OrderBy([("null", Order.Type.Asc)])
            .ExpectErr("Должен упасть с ошибкой");
    }
    
    [Theory]
    [InlineData("'text'")]
    [InlineData("10")]
    [InlineData("10.0")]
    [InlineData("Date(2025,05,12")]
    public void WhereShouldBeOnlyBoolean(string expr)
    {
        var qb = new PostgresAssets().UsersQuery
            .Where(expr)
            .ExpectErr("Должен упасть с ошибкой");
    }
}