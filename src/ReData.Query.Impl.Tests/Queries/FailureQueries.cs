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

    [Fact(DisplayName = "Ссылка на константу, объявленную ниже в Select, должна завершаться ошибкой")]
    public void SelectConstantForwardReferenceShouldFail()
    {
        new PostgresAssets().CreateUsersQuery().Select(new()
        {
            ["Test"] = "const a = b; const b = 1; a",
        }).ExpectErr("Должен упасть с ошибкой");
    }

    [Fact(DisplayName = "Повторное объявление константой в одном выражении Select должно завершаться ошибкой")]
    public void SelectConstantDuplicateNameShouldFail()
    {
        new PostgresAssets().CreateUsersQuery().Select(new()
        {
            ["Test"] = "const a = 1; const a = 2; a",
        }).ExpectErr("Должен упасть с ошибкой");
    }

    [Fact(DisplayName = "Использование неизвестной константой в OrderBy должно завершаться ошибкой")]
    public void OrderByUnknownConstantShouldFail()
    {
        new PostgresAssets().CreateUsersQuery()
            .OrderBy([("missingVar + 1", OrderItem.Type.Asc)])
            .ExpectErr("Должен упасть с ошибкой");
    }

    [Fact(DisplayName = "Ссылка на константу, объявленную позже в GroupBy, должна завершаться ошибкой")]
    public void GroupByConstantForwardReferenceShouldFail()
    {
        new PostgresAssets().CreateUsersQuery()
            .GroupBy(["const a = b; const b = Age; a"], new()
            {
                ["A"] = "const a = b; const b = Age; a",
                ["Count"] = "COUNT()",
            })
            .ExpectErr("Должен упасть с ошибкой");
    }

    [Fact(DisplayName = "Агрегатная константа в Select без runtime должна завершаться ошибкой")]
    public void SelectAggregationConstantWithoutRuntimeShouldFail()
    {
        new PostgresAssets().CreateUsersQuery().Select(new()
        {
            ["Test"] = "const avgAge = AVG(Age); avgAge",
        }).ExpectErr("Должен упасть с ошибкой");
    }

    [Fact(DisplayName = "inline const от поля должен завершаться ошибкой")]
    public void InlineConstFromFieldInSelectShouldFail()
    {
        new PostgresAssets().CreateUsersQuery()
            .Select(new()
            {
                ["Test"] = "const(Age)",
            })
            .ExpectErr("Должен упасть с ошибкой");
    }

    [Fact(DisplayName = "вложенный inline const от поля должен завершаться ошибкой")]
    public void NestedInlineConstFromFieldInSelectShouldFail()
    {
        new PostgresAssets().CreateUsersQuery()
            .Select(new()
            {
                ["Test"] = "const(const(Age))",
            })
            .ExpectErr("Должен упасть с ошибкой");
    }

    [Fact(DisplayName = "Field(const(field)) должен завершаться ошибкой")]
    public void FieldWithInlineConstFromFieldShouldFail()
    {
        new PostgresAssets().CreateUsersQuery()
            .Select(new()
            {
                ["Test"] = "Field(const(Age))",
            })
            .ExpectErr("Должен упасть с ошибкой");
    }

    [Fact(DisplayName = "Field с const-аргументом не должен считаться константой в GroupBy")]
    public void GroupByFieldFunctionShouldNotBeTreatedAsConst()
    {
        // Regression test for Field const-propagation in GroupBy.
        new PostgresAssets().CreateUsersQuery()
            .GroupBy(["Field('FirstName')"], new()
            {
                ["NameText"] = "Field('FirstName')",
                ["Count"] = "COUNT()",
            })
            .Expect("Должен быть валидным запросом");
    }

    [Fact(DisplayName = "const= с inline от неагрегатного поля должен завершаться ошибкой")]
    public void ConstDeclarationWithInlineFromFieldShouldFail()
    {
        new PostgresAssets().CreateUsersQuery()
            .Where("const x = const(Age); x > 0")
            .ExpectErr("Должен упасть с ошибкой");
    }

    [Fact(DisplayName = "inline const внутри inline const с невалидным inner должен завершаться ошибкой")]
    public void MultiInlineWithInvalidInnerShouldFail()
    {
        new PostgresAssets().CreateUsersQuery()
            .Select(new()
            {
                ["Test"] = "const(const(const(Age)))",
            })
            .ExpectErr("Должен упасть с ошибкой");
    }

    [Fact(DisplayName = "(1 + 2).const() не должен поддерживаться")]
    public void MethodStyleConstCallShouldFail()
    {
        new PostgresAssets().CreateUsersQuery()
            .Select(new()
            {
                ["Test"] = "(1 + 2).const()",
            })
            .ExpectErr("Должен упасть с ошибкой");
    }
}
