using FluentAssertions;
using Org.BouncyCastle.Asn1.X509;
using ReData.Query.Impl.Tests.QueryTests;

namespace ReData.Query.Impl.Tests.Queries;

public abstract class Сommon(ISqlRunner runner) : ExprTests(runner)
{
    private IQueryBuilder QueryBuilder => Runner.QueryBuilder;
    [Fact]
    public async Task TableQuery()
    {
        // Arrange
        Query query = Assets.PlayersQuery;
        var sql = QueryBuilder.Build(query);

        // Act
        List<Player> result = await Runner.QueryAsync(sql);

        // Assert
        var expect = Assets.Players;
        result.Should().BeEquivalentTo(expect, options => options.WithStrictOrdering());
    }
    
    [Fact]
    public async Task WhereQuery()
    {
        // Arrange
        Query query = Assets.PlayersQuery.Where("id > 5");
        var sql = QueryBuilder.Build(query);

        // Act
        List<Player> result = await Runner.QueryAsync(sql);

        // Assert
        var expect = Assets.Players.Where(p => p.id > 5);
        result.Should().BeEquivalentTo(expect, options => options.WithStrictOrdering());
    }
    
    [Fact]
    public async Task SelectQuery()
    {
        // Arrange
        Query query = Assets.PlayersQuery.Select(new()
        {
            ["id"] = "id",
            ["Name"] = "Upper(Name)",
            ["MaxScore"] = "MaxScore * 2.0"
        });
        var sql = QueryBuilder.Build(query);

        // Act
        List<Player> result = await Runner.QueryAsync(sql);

        // Assert
        var expect = Assets.Players.Select(p => new Player()
        {
            id = p.id,
            Name = p.Name.ToUpper(),
            MaxScore = p.MaxScore * 2,
        });
        result.Should().BeEquivalentTo(expect, options => options.WithStrictOrdering());
    }
    
    [Fact]
    public async Task OrderByQuery()
    {
        // Arrange
        Query query = Assets.PlayersQuery.OrderByDescending("MaxScore");
        var sql = QueryBuilder.Build(query);

        // Act
        List<Player> result = await Runner.QueryAsync(sql);

        // Assert
        var expect = Assets.Players.OrderBy(p => p.MaxScore);
        result.Should().BeEquivalentTo(expect);
    }
    
    [Fact]
    public async Task OrderByOverride()
    {
        // Arrange
        Query query = Assets.PlayersQuery.OrderBy("Name").OrderByDescending("Name");
        var sql = QueryBuilder.Build(query);

        // Act
        List<Player> result = await Runner.QueryAsync(sql);

        // Assert
        var expect = Assets.Players.OrderByDescending(p => p.Name);
        result.Should().BeEquivalentTo(expect);
    }
    
    [Fact]
    public async Task OrderByMultipleQuery()
    {
        // Arrange
        Query query = Assets.PlayersQuery.OrderBy("MaxScore").ThenByDescending("Name");
        var sql = QueryBuilder.Build(query);

        // Act
        List<Player> result = await Runner.QueryAsync(sql);

        // Assert
        var expect = Assets.Players.OrderBy(p => p.MaxScore).ThenByDescending(p => p.Name);
        result.Should().BeEquivalentTo(expect, options => options.WithStrictOrdering());
    }
    
    [Fact]
    public async Task Limit()
    {
        // Arrange
        Query query = Assets.PlayersQuery.Take(5);
        var sql = QueryBuilder.Build(query);

        // Act
        List<Player> result = await Runner.QueryAsync(sql);

        //Assert
        var expect = Assets.Players.Take(5);
        result.Should().BeEquivalentTo(expect, options => options.WithStrictOrdering());
    }
    
    [Fact]
    public async Task LimitMoreLimitLess()
    {
        // Arrange
        Query query = Assets.PlayersQuery.Take(5).Take(3);
        var sql = QueryBuilder.Build(query);

        // Act
        List<Player> result = await Runner.QueryAsync(sql);

        //Assert
        var expect = Assets.Players.Take(3);
        result.Should().BeEquivalentTo(expect, options => options.WithStrictOrdering());
    }
    
    [Fact]
    public async Task LimitLessLimitMore()
    {
        // Arrange
        Query query = Assets.PlayersQuery.Take(3).Take(5);
        var sql = QueryBuilder.Build(query);

        // Act
        List<Player> result = await Runner.QueryAsync(sql);

        //Assert
        var expect = Assets.Players.Take(3);
        result.Should().BeEquivalentTo(expect, options => options.WithStrictOrdering());
    }
    
    [Fact]
    public async Task LimitThenOrderBy()
    {
        // Arrange
        Query query = Assets.PlayersQuery.Take(5).OrderByDescending("id");
        var sql = QueryBuilder.Build(query);

        // Act
        List<Player> result = await Runner.QueryAsync(sql);

        //Assert
        var expect = Assets.Players.Take(5).OrderByDescending(p => p.id);
        result.Should().BeEquivalentTo(expect, options => options.WithStrictOrdering());
    }
    
    [Fact]
    public async Task LimitThenWhere()
    {
        // Arrange
        Query query = Assets.PlayersQuery.Take(5).Where("Mod(id, 2) = 0");
        var sql = QueryBuilder.Build(query);

        // Act
        List<Player> result = await Runner.QueryAsync(sql);

        //Assert
        var expect = Assets.Players.Take(5).Where(p => p.id % 2 == 0);
        result.Should().BeEquivalentTo(expect, options => options.WithStrictOrdering());
    }
    
    
    // [Fact]
    // public async Task Offset()
    // {
    //     // Arrange
    //     Query query = Assets.PlayersQuery.Skip(5);
    //     var sql = QueryBuilder.Build(query);
    //
    //     // Act
    //     List<Player> result = await Runner.QueryAsync(sql);
    //
    //     // Assert
    //     var expect = Assets.Players.Skip(5);
    //     result.Should().BeEquivalentTo(expect, options => options.WithStrictOrdering());
    // }
    
    // [Fact]
    // public async Task OrderByThenOffset()
    // {
    //     // Arrange
    //     Query query = Assets.PlayersQuery.OrderBy("Name").Skip(5);
    //     var sql = QueryBuilder.Build(query);
    //
    //     // Act
    //     List<Player> result = await Runner.QueryAsync(sql);
    //
    //     // Assert
    //     var expect = Assets.Players.OrderBy(p => p.Name).Skip(5);
    //     result.Should().BeEquivalentTo(expect, options => options.WithStrictOrdering());
    // }
    
    // [Fact]
    // public async Task OffsetThenOrderBy()
    // {
    //     // Arrange
    //     Query query = Assets.PlayersQuery.Skip(5).OrderBy("MaxScore");
    //     var sql = QueryBuilder.Build(query);
    //
    //     // Act
    //     List<Player> result = await Runner.QueryAsync(sql);
    //
    //     // Assert
    //     var expect = Assets.Players.Skip(5).OrderBy(p => p.MaxScore);
    //     result.Should().BeEquivalentTo(expect, options => options.WithStrictOrdering());
    // }
    
    [Fact]
    public async Task LimitOffsetQuery()
    {
        // Arrange
        Query query = Assets.PlayersQuery.Skip(5).Take(5);
        var sql = QueryBuilder.Build(query);

        // Act
        List<Player> result = await Runner.QueryAsync(sql);

        // Assert
        var expect = Assets.Players.Skip(5).Take(5);
        result.Should().BeEquivalentTo(expect, options => options.WithStrictOrdering());
    }
    
    [Fact]
    public async Task SequencialSelect()
    {
        // Arrange
        Query query = Assets.PlayersQuery
        .Select(new()
        {
            ["id"] = "id",
            ["Name"] = "Name",
            ["MaxScore"] = "MaxScore + 1.0"
        }).Select(new()
        {
            ["id"] = "id",
            ["Name"] = "Name",
            ["MaxScore"] = "MaxScore + 1.0"
        }).Select(new()
        {
            ["id"] = "id",
            ["Name"] = "Name",
            ["MaxScore"] = "MaxScore + 1.0"
        });
        
        var sql = QueryBuilder.Build(query);

        // Act
        List<Player> result = await Runner.QueryAsync(sql);

        // Assert
        var expect = Assets.Players.Select(p => p with
        {
            MaxScore = p.MaxScore + 3
        });
        result.Should().BeEquivalentTo(expect);
    }
    
    [Fact]
    public async Task SelectFromNothing()
    {
        // Arrange
        Query query = new Query()
            .Select(new()
            {
                ["id"] = "14",
                ["Name"] = "'Maximus'",
                ["MaxScore"] = "9000.0"
            });
        
        var sql = QueryBuilder.Build(query);

        // Act
        List<Player> result = await Runner.QueryAsync(sql);

        // Assert
        Player[] expect =
        [
            new Player()
            {
                id = 14,
                Name = "Maximus",
                MaxScore = 9000
            }
        ];
        result.Should().BeEquivalentTo(expect);
    }


    // SqlServer
    [Fact]
    public async Task SubqueryWithOrderAndWithoutLimit()
    {
        // Arrange
        Query query = Assets.PlayersQuery
            .OrderByDescending("id")
            .Select(new()
            {
                ["id"] = "id",
                ["Name"] = "Upper(Name) + ' admin'",
                ["MaxScore"] = "-MaxScore"
            });
            
        var sql = QueryBuilder.Build(query);

        // Act
        List<Player> result = await Runner.QueryAsync(sql);

        //Assert
        var expect = Assets.Players
            .OrderByDescending(p => p.id)
            .Select(p => new Player()
            {
                id = p.id,
                Name = p.Name.ToUpper() + " admin",
                MaxScore = -p.MaxScore
            });
        result.Should().BeEquivalentTo(expect, options => options.WithStrictOrdering());
    }
    
    [Fact]
    public async Task FullQuery()
    {
        // Arrange
        Query query = Assets.PlayersQuery
            .OrderByDescending("id")
            .Where("MaxScore > 15.0")
            .Skip(2)
            .Take(5)
            .Select(new()
            {
                ["id"] = "id",
                ["Name"] = "Upper(Name) + ' admin'",
                ["MaxScore"] = "-MaxScore"
            });
            
        var sql = QueryBuilder.Build(query);

        // Act
        List<Player> result = await Runner.QueryAsync(sql);

        //Assert
        var expect = Assets.Players
            .OrderByDescending(p => p.id)
            .Where(p => p.MaxScore > 15)
            .Skip(2)
            .Take(5)
            .Select(p => new Player()
            {
                id = p.id,
                Name = p.Name.ToUpper() + " admin",
                MaxScore = -p.MaxScore
            });
        result.Should().BeEquivalentTo(expect, options => options.WithStrictOrdering());
    }
    
}