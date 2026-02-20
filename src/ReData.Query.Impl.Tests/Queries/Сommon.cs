using FluentAssertions;
using ReData.Common;
using ReData.Query.Core;
using ReData.Query.Core.Types;
using ReData.Query.Impl.Tests.Fixtures;
using ReData.Query.Runners;

// ReSharper disable PossibleMultipleEnumeration

namespace ReData.Query.Impl.Tests.Queries;

#pragma warning disable SA1118

public abstract partial class Сommon(IDatabaseFixture db, ITestAssets assets) : ExprTests(db)
{
    private QueryBuilder CreateUsersQuery() => assets.CreateUsersQuery();

    private async Task<QueryBuilder> CreateUsersQueryWithRuntimeAsync()
    {
        var runner = await db.GetRunnerAsync();
        var runtime = new RunnerConstantRuntime(runner, db.GetConnection());
        return assets.CreateUsersQuery(runtime);
    }

    [Fact]
    public async Task TableQuery()
    {
        // Arrange
        var qb = CreateUsersQuery();

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray.PrepareRecords();

        result.Should().BeEquivalentTo(expect);
    }

    [Fact]
    public async Task WhereQuery()
    {
        // Arrange
        var qb = CreateUsersQuery()
            .Where("UserId > 5")
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray.Where(u => u.UserId > 5).PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task WhereSelectComboQuery()
    {
        // Arrange
        var qb = CreateUsersQuery()
            .Where("UserId > 5")
            .Select(new()
            {
                ["UserId"] = "UserId * 2"
            })
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray
            .Where(u => u.UserId > 5)
            .Select(u => new
            {
                UserId = u.UserId * 2
            })
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }


    [Fact]
    public async Task OrderBySelectComboQuery()
    {
        // Arrange
        var qb = CreateUsersQuery()
            .OrderBy([("UserId", OrderItem.Type.Asc)])
            .Select(new()
            {
                ["UserId"] = "Mod(UserId, 2)"
            })
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray
            .OrderBy(u => u.UserId)
            .Select(u => new
            {
                UserId = u.UserId % 2,
            }).PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task SelectQuery()
    {
        // Arrange
        var qb = CreateUsersQuery().Select(new()
        {
            ["id"] = "UserId",
            ["Name"] = "Upper(FirstName + LastName)",
            ["DoubleAge"] = "2 * Age",
            ["Age"] = "Age"
        })
        .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray.Select(u => new
        {
            id = u.UserId,
            Name = (u.FirstName + u.LastName).ToUpper(),
            DoubleAge = u.Age * 2,
            u.Age
        }).PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }
    
    [Fact]
    public async Task QueryWithInterpolation()
    {
        // Arrange
        var qb = CreateUsersQuery()
            .Select(new()
            {
                ["FullName"] = "'{FirstName} {LastName}'",
                ["DoubleAge"] = "2 * Age",
                ["Age"] = "Age"
            })
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray.Select(u => new
        {
            FullName = (u.FirstName + " " + u.LastName).ToUpper(),
            DoubleAge = u.Age * 2,
            u.Age
        }).PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task OrderByQuery()
    {
        // Arrange
        var qb = CreateUsersQuery()
            .OrderBy([("Salary", OrderItem.Type.Desc)])
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray.OrderByDescending(u => u.Salary).PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task OrderByOverride()
    {
        // Arrange
        var qb = CreateUsersQuery()
            .OrderBy([("Salary", OrderItem.Type.Desc)])
            .OrderBy([("FirstName", OrderItem.Type.Asc)])
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray.OrderBy(u => u.FirstName).PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task OrderByMultipleQuery()
    {
        // Arrange
        var qb = CreateUsersQuery()
            .OrderBy([
                ("Notes", OrderItem.Type.Asc),
                ("FirstName", OrderItem.Type.Asc)
            ])
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray.OrderBy(u => u.Notes).ThenBy(u => u.FirstName).PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Theory(DisplayName = "Order by contant values is irrelevant")]
    [InlineData("-1")]
    [InlineData("100")]
    [InlineData("'text'")]
    // [InlineData("true")] Order сейчас не может содержать бул
    // [InlineData("NULL")] Order сейчас не может содержать нул
    [InlineData("UserId.Type()")]
    [InlineData("JoinDate.Type()")]
    [InlineData("Now()")]
    [InlineData("Today()")]
    public async Task OrderByNegOne(string expr)
    {
        // Arrange
        var qb = CreateUsersQuery()
            .OrderBy([(expr, OrderItem.Type.Asc)])
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray.PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }


    [Fact]
    public async Task Limit()
    {
        // Arrange
        var qb = CreateUsersQuery().Take(5);

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray.Take(5).PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task LimitGetOne()
    {
        // Arrange
        var qb = CreateUsersQuery().Take(1);

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray.Take(1).PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task LimitMoreLimitLess()
    {
        // Arrange
        var qb = CreateUsersQuery().Take(5).Take(3);

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray.Take(3).PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task LimitLessLimitMore()
    {
        // Arrange
        var qb = CreateUsersQuery().Take(3).Take(5);

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray.Take(3).PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task LimitThenOrderBy()
    {
        // Arrange
        var qb = CreateUsersQuery()
            .Take(5)
            .OrderBy([("UserId", OrderItem.Type.Desc)])
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray.Take(5).OrderByDescending(u => u.UserId).PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task LimitThenWhere()
    {
        // Arrange
        var qb = CreateUsersQuery()
            .Take(5)
            .Where("Mod(UserId,2) = 0")
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray
            .Take(5)
            .Where(u => u.UserId % 2 == 0)
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }


    [Fact]
    public async Task Offset()
    {
        // Arrange
        var qb = CreateUsersQuery().Skip(5);

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray.Skip(5).PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task LimitOffsetQuery()
    {
        // Arrange
        var qb = CreateUsersQuery().Skip(5).Take(5);

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray.Skip(5).Take(5).PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Sequential limitOffset should preserve slicing semantics")]
    [Trait("Issue", "https://github.com/MikeKondr99/ReData/issues/113")]
    public async Task LimitOffsetAfterLimitOffset_ShouldApplySequentially()
    {
        // Regression test for GH-113.
        // Arrange
        var qb = CreateUsersQuery()
            .Skip(5)
            .Take(10)
            .Skip(7)
            .Take(10);

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray
            .Skip(5)
            .Take(10)
            .Skip(7)
            .Take(10)
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact(DisplayName = "Sequential limitOffset should treat zero-limit state as no explicit limit")]
    [Trait("Issue", "https://github.com/MikeKondr99/ReData/issues/113")]
    public async Task LimitOffsetAfterLimitOffset_WithOffsetBeyondLimitedData_ShouldFollowCurrentZeroAsNoLimitContract()
    {
        // Regression test for GH-113.
        // Arrange
        var qb = CreateUsersQuery()
            .Take(10)
            .Skip(10)
            .Take(10);

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray
            .Skip(10)
            .Take(10)
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task BugWhereLeak()
    {
        // Arrange
        var qb = CreateUsersQuery()
            .Where("UserId > 5")
            .Select(new()
            {
                ["Поле"] = "100"
            })
            .Where("'10' = 10.Text()")
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray
            .Where(u => u.UserId > 5)
            .Select(_ => new
            {
                Поле = 100,
            })
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task SequencialSelect()
    {
        // Arrange
        var qb = CreateUsersQuery().Select(new()
            {
                ["id"] = "UserId",
                ["Name"] = "FirstName",
                ["Age"] = "Age + 1",
            })
            .Select(new()
            {
                ["id"] = "id",
                ["Name"] = "Name",
                ["Age"] = "Age + 1",
            })
            .Select(new()
            {
                ["id"] = "id",
                ["Name"] = "Name",
                ["Age"] = "Age + 1",
            })
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray
            .Select(u => new
            {
                id = u.UserId,
                Name = u.FirstName,
                Age = u.Age + 3,
            })
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task SelectFromNothing()
    {
        // Arrange
        var qb = QueryBuilder.FromDual(Factory.CreateExpressionResolver(assets.DatabaseType), Factory.CreateFunctionStorage(assets.DatabaseType))
            .Select(new()
            {
                ["id"] = "14",
                ["Name"] = "'Maximus'",
                ["MaxScore"] = "9000.0",
            })
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        dynamic[] expect =
        [
            new
            {
                id = 14,
                Name = "Maximus",
                MaxScore = 9000.0,
            }
        ];

        result.Should().BeEquivalentTo(expect.PrepareRecords(), o => o.WithStrictOrdering());
    }


    // SqlServer
    [Fact]
    public async Task SubqueryWithOrderAndWithoutLimit()
    {
        // Arrange
        var qb = CreateUsersQuery()
            .OrderBy([("UserId", OrderItem.Type.Desc)])
            .Select(new()
            {
                ["id"] = "UserId",
                ["Name"] = "Upper(FirstName) + ' admin'",
                ["AntiSalary"] = "-Salary",
            })
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray
            .OrderByDescending(u => u.UserId)
            .Select(u => new
            {
                id = u.UserId,
                Name = u.FirstName.ToUpper() + " admin",
                AntiSalary = -u.Salary,
            })
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task FullQuery()
    {
        // Arrange
        var qb = CreateUsersQuery()
            .OrderBy([("UserId", OrderItem.Type.Desc)])
            .Where("Salary > 30000.0").UnwrapOk().Value
            .Skip(2)
            .Take(5)
            .Select(new()
            {
                ["id"] = "UserId",
                ["Name"] = "Upper(FirstName) + ' admin'",
                ["AntiSalary"] = "-Salary",
            })
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray
            .OrderByDescending(u => u.UserId)
            .Where(u => u.Salary > 30000.0)
            .Skip(2)
            .Take(5)
            .Select(u => new
            {
                id = u.UserId,
                Name = u.FirstName.ToUpper() + " admin",
                AntiSalary = -u.Salary,
            })
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task GroupByWithOnlyGroup()
    {
        // Arrange
        var qb = CreateUsersQuery()
            .GroupBy(["Notes"], new()
            {
                ["TEST"] = "Notes"
            })
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray
            .Select(u => u.Notes)
            .Distinct()
            .Select(n => new
            {
                TEST = n,
            })
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task GroupByWithAggregations()
    {
        // Arrange
        var qb = CreateUsersQuery()
            .GroupBy(["Notes"], new()
            {
                ["TEST"] = "Notes",
                ["Sum"] = "SUM(Salary)",
                ["Avg"] = "AVG(Salary)",
                ["Min"] = "MIN(Salary)",
                ["Max"] = "MAX(Salary)",
            })
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray
            .GroupBy(u => u.Notes, (k, v) => new
            {
                TEST = k,
                Sum = v.Select(u => (double)u.Salary).Sum(),
                Avg = v.Select(u => (double)u.Salary).Average(),
                Min = v.Select(u => u.Salary).Min(),
                Max = v.Select(u => u.Salary).Max(),
            })
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }


    [Fact]
    public async Task GroupByMultipleKeys()
    {
        // Arrange
        var qb = CreateUsersQuery()
            .GroupBy(["FirstName", "LastName"], new()
            {
                ["FirstName"] = "FirstName",
                ["LastName"] = "LastName",
                ["Count"] = "COUNT()",
                ["TotalSalary"] = "SUM(Salary)"
            })
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray
            .GroupBy(u => new
            {
                u.FirstName,
                u.LastName
            }, (k, v) => new
            {
                k.FirstName,
                k.LastName,
                Count = v.Count(),
                TotalSalary = v.Sum(u => (double)u.Salary)
            })
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task GroupByWithHaving()
    {
        // Arrange
        var qb = CreateUsersQuery()
            .GroupBy(["Notes"], new()
            {
                ["Note"] = "Notes",
                ["AvgSalary"] = "AVG(Salary)"
            })
            .Where("AvgSalary > 60000")
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray
            .GroupBy(u => u.Notes, (k, v) => new
            {
                Note = k,
                AvgSalary = v.Average(u => (double)u.Salary)
            })
            .Where(g => g.AvgSalary > 60000)
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task GroupByIntegerDivisionInHavingUsesIntegerSemantics()
    {
        // Arrange
        var qb = CreateUsersQuery()
            .GroupBy(["Mod(UserId, 2)"], new()
            {
                ["Bucket"] = "Mod(UserId, 2)",
                ["ratio"] = "SUM(Age) / COUNT()",
            })
            .Where("ratio > 31.5")
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        dynamic[] expect =
        [
            new
            {
                Bucket = 1,
                ratio = 32,
            }
        ];

        result.Should().BeEquivalentTo(expect.PrepareRecords(), o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task GroupByWithWhereBefore()
    {
        // Arrange
        var qb = CreateUsersQuery()
            .GroupBy(["FirstName"], new()
            {
                ["Name"] = "FirstName",
                ["MaxMinDiff"] = "MAX(Salary) - MIN(Salary)",
                ["AvgAge"] = "AVG(Age)",
                ["TotalUsers"] = "COUNT()"
            })
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray
            .GroupBy(u => u.FirstName, (k, v) => new
            {
                Name = k,
                MaxMinDiff = v.Max(u => u.Salary) - v.Min(u => u.Salary),
                AvgAge = v.Average(u => u.Age),
                TotalUsers = v.Count()
            })
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task GroupByWithOrderBy()
    {
        // Arrange
        var qb = CreateUsersQuery()
            .GroupBy(["Notes"], new()
            {
                ["Note"] = "Notes",
                ["UserCount"] = "COUNT()"
            })
            .OrderBy([("UserCount", OrderItem.Type.Desc)])
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray
            .GroupBy(u => u.Notes, (k, v) => new
            {
                Note = k,
                UserCount = v.Count()
            })
            .OrderByDescending(g => g.UserCount)
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [SkippableFact]
    public async Task GroupByWithLimit()
    {
        Skip.If(assets.DatabaseType == DatabaseType.Oracle, "TODO починить позже");
        // Arrange
        var qb = CreateUsersQuery()
            .GroupBy(["Notes"], new()
            {
                ["Note"] = "Notes",
                ["UserCount"] = "COUNT()"
            })
            .Expect("Valid query")
            .Take(3);

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray
            .GroupBy(u => u.Notes, (k, v) => new
            {
                Note = k,
                UserCount = v.Count()
            })
            .Take(3)
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task GroupByWithDatePart()
    {
        // Arrange
        var qb = CreateUsersQuery()
            .GroupBy(["Year(JoinDate)"], new()
            {
                ["JoinYear"] = "Year(JoinDate)",
                ["UserCount"] = "COUNT()",
                ["AvgSalary"] = "AVG(Salary)"
            })
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray
            .GroupBy(u => u.JoinDate.Year, (k, v) => new
            {
                JoinYear = k,
                UserCount = v.Count(),
                AvgSalary = v.Average(u => (double)u.Salary)
            })
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task GroupByWithCaseExpression()
    {
        // Arrange
        var qb = CreateUsersQuery()
            .GroupBy(["If(Age > 30,'Senior','Junior')"], new()
            {
                ["AgeGroup"] = "If(Age > 30,'Senior','Junior')",
                ["Count"] = "COUNT()",
                ["MaxSalary"] = "MAX(Salary)"
            })
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray
            .GroupBy(u => u.Age > 30 ? "Senior" : "Junior", (k, v) => new
            {
                AgeGroup = k,
                Count = v.Count(),
                MaxSalary = v.Max(u => u.Salary)
            })
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }
    
    [Fact]
    public async Task ConcatWithOrder()
    {
        // Arrange
        var qb = CreateUsersQuery()
            .Select(new ()
            {
               ["concat"] = "CONCAT(FirstName,', ', Age)"
            })
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        dynamic[] data = [ new { concat = assets.UsersDynamicArray.Select(u => u.FirstName).JoinBy(", ") }];
        var expect = data.PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }
    
    [Fact]
    public async Task GroupByWithConcatWithOrder()
    {
        // Arrange
        var qb = CreateUsersQuery()
            .GroupBy(["Notes"], new ()
            {
               ["concat"] = "CONCAT(FirstName,', ', Age)"
            })
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray
            .GroupBy(u => u.Notes, (_, v) => new
            {
                concat = v.OrderBy(u => u.Age).Select(u => (string)u.FirstName).JoinBy(", ")
            })
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }
    
    
    // Баг найден Данилой Карбушевым
    // Sort
    // Limit 
    // Where
    [Fact]
    public async Task TestToTrackBug85()
    {
        // Arrange
        var qb = CreateUsersQuery()
            .OrderBy([
                ("UserId", OrderItem.Type.Desc)
            ]).Expect(e => e.JoinBy(", "))
            .Take(10)
            .Where("Age < 100")
            .Expect("Valid query");

        // Act
        var result = await GetObjectsAsync(qb);

        // Assert
        var expect = assets.UsersDynamicArray.OrderBy(u => u.UserId).Take(10).PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }
    
    
}

