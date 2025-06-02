using FluentAssertions;
using ReData.Common;
using ReData.Query.Core;
using ReData.Query.Core.Types;
using ReData.Query.Impl.Tests.Fixtures;
using ReData.Query.Runners.Value;

namespace ReData.Query.Impl.Tests.Queries;

public static class RecordsTestHelper
{
    public static IEnumerable<Dictionary<string, IValue>> PrepareRecords(this IEnumerable<dynamic> objects)
    {
        return objects.Select(ConvertDynamicToIValueDictionary);
    }

    public static Dictionary<string, IValue> ConvertDynamicToIValueDictionary(dynamic dynamicObject)
    {
        if (dynamicObject == null)
        {
            throw new ArgumentNullException(nameof(dynamicObject));
        }

        var dictionary = new Dictionary<string, IValue>();

        // Handle ExpandoObject which is often used with dynamic
        if (dynamicObject is IDictionary<string, object> expandoDict)
        {
            foreach (var kvp in expandoDict)
            {
                dictionary[kvp.Key] = ConvertToIValue(kvp.Value);
            }
        }
        else
        {
            // Handle regular objects using reflection
            var properties = dynamicObject.GetType().GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(dynamicObject);
                dictionary[property.Name] = ConvertToIValue(value);
            }
        }

        return dictionary;
    }

    private static IValue ConvertToIValue(object? value)
    {
        if (value is null)
        {
            return default(NullValue);
        }

        switch (value)
        {
            case string str:
                return new TextValue(str);
            case bool b:
                return new BoolValue(b);
            case int i:
                return new IntegerValue(i);
            case long l:
                return new IntegerValue(Convert.ToInt32(l)); // or handle as separate case if you have LongValue
            case double d:
                return new NumberValue(d);
            case float f:
                return new NumberValue(Convert.ToDouble(f));
            case decimal dec:
                return new NumberValue(Convert.ToDouble(dec));
            case DateTime dt:
                return new DateTimeValue(dt);
            default:
                throw new InvalidOperationException($"Unsupported type: {value.GetType().FullName}");
        }
    }
}

public abstract class Сommon(IDatabaseFixture db, ITestAssets assets) : ExprTests(db)
{
    [Fact]
    public async Task TableQuery()
    {
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery;

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.Build());

        // Assert
        var expect = assets.UsersDynamicArray.PrepareRecords();

        result.Should().BeEquivalentTo(expect);
    }

    [Fact]
    public async Task WhereQuery()
    {
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery.Where("UserId > 5");

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.Expect("Valid query").Build());

        // Assert
        var expect = assets.UsersDynamicArray.Where(u => u.UserId > 5).PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task WhereSelectComboQuery()
    {
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery
            .Where("UserId > 5")
            .Select(new()
            {
                ["UserId"] = "UserId * 2"
            });

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.Expect("Valid query").Build());

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
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery
            .OrderBy([("UserId", OrderItem.Type.Asc)])
            .Select(new()
            {
                ["UserId"] = "Mod(UserId, 2)"
            });

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.Expect("Valid query").Build());

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
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery.Select(new()
        {
            ["id"] = "UserId",
            ["Name"] = "Upper(FirstName + LastName)",
            ["DoubleAge"] = "2 * Age",
            ["Age"] = "Age"
        });

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.Expect("Valid query").Build());

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
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery.Select(new()
        {
            ["FullName"] = "'{FirstName} {LastName}'",
            ["DoubleAge"] = "2 * Age",
            ["Age"] = "Age"
        });

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.Expect("Valid query").Build());

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
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery.OrderBy([("Salary", OrderItem.Type.Desc)]);

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.Expect("Valid query").Build());

        // Assert
        var expect = assets.UsersDynamicArray.OrderByDescending(u => u.Salary).PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task OrderByOverride()
    {
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery
            .OrderBy([("Salary", OrderItem.Type.Desc)])
            .OrderBy([("FirstName", OrderItem.Type.Asc)]);

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.Unwrap().Build());

        // Assert
        var expect = assets.UsersDynamicArray.OrderBy(u => u.FirstName).PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task OrderByMultipleQuery()
    {
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery
            .OrderBy([
                ("Notes", OrderItem.Type.Asc),
                ("FirstName", OrderItem.Type.Asc)
            ]);

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.Unwrap().Build());

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
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery
            .OrderBy([(expr, OrderItem.Type.Asc)]);

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.Unwrap().Build());

        // Assert
        var expect = assets.UsersDynamicArray.PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }


    [Fact]
    public async Task Limit()
    {
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery.Take(5);

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.Build());

        // Assert
        var expect = assets.UsersDynamicArray.Take(5).PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task LimitGetOne()
    {
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery.Take(1);

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.Build());

        // Assert
        var expect = assets.UsersDynamicArray.Take(1).PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task LimitMoreLimitLess()
    {
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery.Take(5).Take(3);

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.Build());

        // Assert
        var expect = assets.UsersDynamicArray.Take(3).PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task LimitLessLimitMore()
    {
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery.Take(3).Take(5);

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.Build());

        // Assert
        var expect = assets.UsersDynamicArray.Take(3).PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task LimitThenOrderBy()
    {
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery.Take(5).OrderBy([("UserId", OrderItem.Type.Desc)]);

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.Unwrap().Build());

        // Assert
        var expect = assets.UsersDynamicArray.Take(5).OrderByDescending(u => u.UserId).PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task LimitThenWhere()
    {
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery.Take(5).Where("Mod(UserId,2) = 0");

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.Unwrap().Build());

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
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery.Skip(5);

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.Build());

        // Assert
        var expect = assets.UsersDynamicArray.Skip(5).PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task LimitOffsetQuery()
    {
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery.Skip(5).Take(5);

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.Build());

        // Assert
        var expect = assets.UsersDynamicArray.Skip(5).Take(5).PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task BugWhereLeak()
    {
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery
            .Where("UserId > 5")
            .Select(new()
            {
                ["Поле"] = "100"
            })
            .Where("'10' = 10.Text()");

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.Expect("ValidQuery").Build());

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
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery.Select(new()
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
            });

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.UnwrapOk().Value.Build());

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
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = QueryBuilder.FromDual(Factory.CreateExpressionResolver(assets.DatabaseType))
            .Select(new()
            {
                ["id"] = "14",
                ["Name"] = "'Maximus'",
                ["MaxScore"] = "9000.0",
            });

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.UnwrapOk().UnwrapOk().Value.Build());

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
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery
            .OrderBy([("UserId", OrderItem.Type.Desc)])
            .Select(new()
            {
                ["id"] = "UserId",
                ["Name"] = "Upper(FirstName) + ' admin'",
                ["AntiSalary"] = "-Salary",
            });

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.UnwrapOk().Value.Build());

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
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery
            .OrderBy([("UserId", OrderItem.Type.Desc)])
            .Where("Salary > 30000.0").UnwrapOk().Value
            .Skip(2)
            .Take(5)
            .Select(new()
            {
                ["id"] = "UserId",
                ["Name"] = "Upper(FirstName) + ' admin'",
                ["AntiSalary"] = "-Salary",
            });

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.UnwrapOk().Value.Build());

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
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery
            .GroupBy(["Notes"], new()
            {
                ["TEST"] = "Notes"
            });

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.UnwrapOk().Value.Build());

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
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery
            .GroupBy(["Notes"], new()
            {
                ["TEST"] = "Notes",
                ["Sum"] = "SUM(Salary)",
                ["Avg"] = "AVG(Salary)",
                ["Min"] = "MIN(Salary)",
                ["Max"] = "MAX(Salary)",
            });

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.UnwrapOk().Value.Build());

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
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery
            .GroupBy(["FirstName", "LastName"], new()
            {
                ["FirstName"] = "FirstName",
                ["LastName"] = "LastName",
                ["Count"] = "COUNT()",
                ["TotalSalary"] = "SUM(Salary)"
            });

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.UnwrapOk().Value.Build());

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
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery
            .GroupBy(["Notes"], new()
            {
                ["Note"] = "Notes",
                ["AvgSalary"] = "AVG(Salary)"
            })
            .Where("AvgSalary > 60000");

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.UnwrapOk().Value.Build());

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
    public async Task GroupByWithWhereBefore()
    {
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery
            .Where("Age > 30")
            .GroupBy(["Notes"], new()
            {
                ["Note"] = "Notes",
                ["UserCount"] = "COUNT()"
            });

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.Unwrap().Build());

        // Assert
        var expect = assets.UsersDynamicArray
            .Where(u => u.Age > 30)
            .GroupBy(u => u.Notes, (k, v) => new
            {
                Note = k,
                UserCount = v.Count()
            })
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task GroupByWithComplexAggregation()
    {
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery
            .GroupBy(["FirstName"], new()
            {
                ["Name"] = "FirstName",
                ["MaxMinDiff"] = "MAX(Salary) - MIN(Salary)",
                ["AvgAge"] = "AVG(Age)",
                ["TotalUsers"] = "COUNT()"
            });

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.Unwrap().Build());

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
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery
            .GroupBy(["Notes"], new()
            {
                ["Note"] = "Notes",
                ["UserCount"] = "COUNT()"
            })
            .OrderBy([("UserCount", OrderItem.Type.Desc)]);

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.UnwrapOk().Value.Build());

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
        var runner = await db.GetRunnerAsync();
        Skip.If(assets.DatabaseType == DatabaseType.Oracle, "TODO починить позже");
        // Arrange
        var qb = assets.UsersQuery
            .GroupBy(["Notes"], new()
            {
                ["Note"] = "Notes",
                ["UserCount"] = "COUNT()"
            })
            .Unwrap()
            .Take(3);

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.Build());

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
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery
            .GroupBy(["Year(JoinDate)"], new()
            {
                ["JoinYear"] = "Year(JoinDate)",
                ["UserCount"] = "COUNT()",
                ["AvgSalary"] = "AVG(Salary)"
            });

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.UnwrapOk().Value.Build());

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
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery
            .GroupBy(["If(Age > 30,'Senior','Junior')"], new()
            {
                ["AgeGroup"] = "If(Age > 30,'Senior','Junior')",
                ["Count"] = "COUNT()",
                ["MaxSalary"] = "MAX(Salary)"
            });

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.UnwrapOk().Value.Build());

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
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery
            .Select(new ()
            {
               ["concat"] = "CONCAT(FirstName,', ', Age)"
            });

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.UnwrapOk().Value.Build());

        // Assert
        dynamic[] data = [ new { concat = assets.UsersDynamicArray.Select(u => u.FirstName).JoinBy(", ") }];
        var expect = data.PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }
    
    [Fact]
    public async Task GroupByWithConcatWithOrder()
    {
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery
            .GroupBy(["Notes"], new ()
            {
               ["concat"] = "CONCAT(FirstName,', ', Age)"
            });

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.UnwrapOk().Value.Build());

        // Assert
        var expect = assets.UsersDynamicArray
            .GroupBy(u => u.Notes, (_, v) => new
            {
                concat = v.OrderBy(u => u.Age).Select(u => (string)u.FirstName).JoinBy(", ")
            })
            .PrepareRecords();

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }
}