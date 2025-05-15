using FluentAssertions;
using ReData.Query.Core;
using ReData.Query.Core.Types;
using ReData.Query.Impl.Tests.Fixtures;
using ReData.Query.Runners.Value;

namespace ReData.Query.Impl.Tests.Queries;

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
        var expect = assets.UsersData;

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
        var expect = assets.UsersData.Where(u => u["UserId"] is IntegerValue(> 5));

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
        var expect = assets.UsersData
            .Where(u => u["UserId"] is IntegerValue(> 5))
            .Select(u => new Dictionary<string, IValue>()
            {
                ["UserId"] = new IntegerValue(u.Int("UserId").Value * 2),
            });

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }
    
    
    [Fact]
    public async Task OrderBySelectComboQuery()
    {
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery
            .OrderBy([("UserId",Order.Type.Asc)])
            .Select(new()
            {
                ["UserId"] = "Mod(UserId, 2)"
            });

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.Expect("Valid query").Build());

        // Assert
        var expect = assets.UsersData
            .OrderBy(u => u.Int("UserId")!.Value)
            .Select(u => new Dictionary<string, IValue>()
            {
                ["UserId"] = new IntegerValue(u.Int("UserId")!.Value % 2),
            });

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
        var expect = assets.UsersData.Select(u => new Dictionary<string, IValue>()
        {
            ["id"] = u["UserId"],
            ["Name"] = new TextValue((u.Text("FirstName") + u.Text("LastName")).ToUpper()),
            ["DoubleAge"] = new IntegerValue(u.Int("Age").Value * 2),
            ["Age"] = u["Age"]
        });

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task OrderByQuery()
    {
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery.OrderBy([("Salary", Order.Type.Desc)]);

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.Expect("Valid query").Build());

        // Assert
        var expect = assets.UsersData.OrderByDescending(u => u.Num("Salary"));

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task OrderByOverride()
    {
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery
            .OrderBy([("Salary", Order.Type.Desc)])
            .OrderBy([("FirstName", Order.Type.Asc)]);

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.UnwrapOk().Value.Build());

        // Assert
        var expect = assets.UsersData.OrderBy(u => u.Text("FirstName"));

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task OrderByMultipleQuery()
    {
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery
            .OrderBy([("Notes", Order.Type.Asc)])
            .OrderBy([("FirstName", Order.Type.Asc)]);

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.UnwrapOk().Value.Build());

        // Assert
        var expect = assets.UsersData.OrderBy(u => u.Text("Notes")).ThenBy(u => u.Text("FirstName"));

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
            .OrderBy([(expr, Order.Type.Asc)]);

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.UnwrapOk().Value.Build());

        // Assert
        var expect = assets.UsersData;

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
        var expect = assets.UsersData.Take(5);

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
        var expect = assets.UsersData.Take(1);

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
        var expect = assets.UsersData.Take(3);

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
        var expect = assets.UsersData.Take(3);

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task LimitThenOrderBy()
    {
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery.Take(5).OrderBy([("UserId", Order.Type.Desc)]);

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.UnwrapOk().Value.Build());

        // Assert
        var expect = assets.UsersData.Take(5).OrderByDescending(u => u.Int("UserId"));

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task LimitThenWhere()
    {
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery.Take(5).Where("Mod(UserId,2) = 0");

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.UnwrapOk().Value.Build());

        // Assert
        var expect = assets.UsersData.Take(5).Where(u => u.Int("UserId").Value % 2 == 0);

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
        var expect = assets.UsersData.Skip(5);

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
        var expect = assets.UsersData.Skip(5).Take(5);

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
        var expect = assets.UsersData.Select(u => new Dictionary<string, IValue>()
        {
            ["id"] = u["UserId"],
            ["Name"] = u["FirstName"],
            ["Age"] = new IntegerValue(u.Int("Age").Value + 3),
        });

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }

    [Fact]
    public async Task SelectFromNothing()
    {
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = QueryBuilder.FromDual(new Factory().CreateExpressionResolver(assets.DatabaseType))
            .Select(new()
            {
                ["id"] = "14",
                ["Name"] = "'Maximus'",
                ["MaxScore"] = "9000.0",
            });

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.UnwrapOk().UnwrapOk().Value.Build());

        // Assert
        Dictionary<string,IValue>[] expect =
        [
            new Dictionary<string, IValue>()
            {
                ["id"] = new IntegerValue(14),
                ["Name"] = new TextValue("Maximus"),
                ["MaxScore"] = new NumberValue(9000.0)
            }
        ];

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }


     // SqlServer
     [Fact]
     public async Task SubqueryWithOrderAndWithoutLimit()
     {
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery
            .OrderBy([("UserId", Order.Type.Desc)])
            .Select(new()
        {
            ["id"] = "UserId",
            ["Name"] = "Upper(FirstName) + ' admin'",
            ["AntiSalary"] = "-Salary",
        });

        // Act
        var result = await runner.RunQueryAsObjectAsync(qb.UnwrapOk().Value.Build());

        // Assert
        var expect = assets.UsersData.OrderByDescending(u => u.Int("UserId")).Select(u => new Dictionary<string, IValue>()
        {
            ["id"] = u["UserId"],
            ["Name"] = new TextValue(u.Text("FirstName").ToUpper() + " admin"),
            ["AntiSalary"] = new NumberValue(-u.Num("Salary").Value),
        });

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
     }
    
    [Fact]
    public async Task FullQuery()
    {
        var runner = await db.GetRunnerAsync();
        // Arrange
        var qb = assets.UsersQuery
            .OrderBy([("UserId", Order.Type.Desc)])
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
        var expect = assets.UsersData
            .OrderByDescending(u => u.Int("UserId"))
            .Where(u => u.Num("Salary").Value > 30000.0)
            .Skip(2)
            .Take(5)
            .Select(u => new Dictionary<string, IValue>()
        {
            ["id"] = u["UserId"],
            ["Name"] = new TextValue(u.Text("FirstName").ToUpper() + " admin"),
            ["AntiSalary"] = new NumberValue(-u.Num("Salary").Value),
        });

        result.Should().BeEquivalentTo(expect, o => o.WithStrictOrdering());
    }
    
    
}