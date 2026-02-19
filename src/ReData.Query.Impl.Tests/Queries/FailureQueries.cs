using FluentAssertions;
using Mysqlx.Expr;
using ReData.Query.Common;
using ReData.Query.Core;
using ReData.Query.Core.Types;

namespace ReData.Query.Impl.Tests.Queries;

#pragma warning disable SA1118

public class FailureQueries
{
    [Fact]
    public void CombinationOfAggrAndNonConstShouldFail()
    {
        var qb = new PostgresAssets().CreateUsersQuery().Select(new()
        {
            ["Test"] = "MIN(FirstName) + LastName",
        }).ExpectErr("Должен упасть с ошибкой");
    }

    [Fact]
    public void CombinationOfAggrFieldAndNotAggrFieldShouldFail()
    {
        var qb = new PostgresAssets().CreateUsersQuery().Select(new()
        {
            ["Test"] = "MIN(FirstName)",
            ["Test2"] = "LastName",
        }).ExpectErr("Должен упасть с ошибкой");
    }

    [Fact]
    public void SelectMustBeNonBoolean()
    {
        var qb = new PostgresAssets().CreateUsersQuery().Select(new()
        {
            ["Test"] = "10 = 10",
        }).ExpectErr("Должен упасть с ошибкой");
    }

    [Fact]
    public void SelectMustBeNonNull()
    {
        var qb = new PostgresAssets().CreateUsersQuery().Select(new()
        {
            ["Test"] = "null",
        }).ExpectErr("Должен упасть с ошибкой");
    }

    [Fact]
    public void OrderByMustBeNonBoolean()
    {
        var qb = new PostgresAssets().CreateUsersQuery()
            .OrderBy([("10 = 10", OrderItem.Type.Asc)])
            .ExpectErr("Должен упасть с ошибкой");
    }

    [Fact]
    public void OrderByMustBeNonNull()
    {
        var qb = new PostgresAssets().CreateUsersQuery()
            .OrderBy([("null", OrderItem.Type.Asc)])
            .ExpectErr("Должен упасть с ошибкой");
    }

    [Fact]
    public void ErrorInGroupByFieldMustNotDupError()
    {
        IEnumerable<IReadOnlyList<ExprError>>? errors = new PostgresAssets().CreateUsersQuery()
            .GroupBy([
                "NotViableField"
            ], new()
            {
                ["Group1"] = "NotViableField",
                ["sum"] = "Sum(UserId)",
            })
            .ExpectErr("Должен упасть с ошибкой");

        errors.Should().HaveCount(2);
    }

    [Theory]
    [InlineData("'text'")]
    [InlineData("10")]
    [InlineData("10.0")]
    [InlineData("Date(2025,05,12")]
    public void WhereShouldBeOnlyBoolean(string expr)
    {
        var qb = new PostgresAssets().CreateUsersQuery()
            .Where(expr)
            .ExpectErr("Должен упасть с ошибкой");
    }

    [Fact(DisplayName = "Ссылка на переменную, объявленную ниже в Select, должна завершаться ошибкой")]
    public void SelectVariableForwardReferenceShouldFail()
    {
        new PostgresAssets().CreateUsersQuery().Select(new()
        {
            ["Test"] = "var a = b; var b = 1; a",
        }).ExpectErr("Должен упасть с ошибкой");
    }

    [Fact(DisplayName = "Повторное объявление переменной в одном выражении Select должно завершаться ошибкой")]
    public void SelectVariableDuplicateNameShouldFail()
    {
        new PostgresAssets().CreateUsersQuery().Select(new()
        {
            ["Test"] = "var a = 1; var a = 2; a",
        }).ExpectErr("Должен упасть с ошибкой");
    }

    [Fact(DisplayName = "Использование неизвестной переменной в OrderBy должно завершаться ошибкой")]
    public void OrderByUnknownVariableShouldFail()
    {
        new PostgresAssets().CreateUsersQuery()
            .OrderBy([("missingVar + 1", OrderItem.Type.Asc)])
            .ExpectErr("Должен упасть с ошибкой");
    }

    [Fact(DisplayName = "Ссылка на переменную, объявленную позже в GroupBy, должна завершаться ошибкой")]
    public void GroupByVariableForwardReferenceShouldFail()
    {
        new PostgresAssets().CreateUsersQuery()
            .GroupBy(["var a = b; var b = Age; a"], new()
            {
                ["A"] = "var a = b; var b = Age; a",
                ["Count"] = "COUNT()",
            })
            .ExpectErr("Должен упасть с ошибкой");
    }

    [Fact(DisplayName = "Агрегатная переменная в Select без runtime должна завершаться ошибкой")]
    public void SelectAggregationVariableWithoutRuntimeShouldFail()
    {
        new PostgresAssets().CreateUsersQuery().Select(new()
        {
            ["Test"] = "var avgAge = AVG(Age); avgAge",
        }).ExpectErr("Должен упасть с ошибкой");
    }
}
