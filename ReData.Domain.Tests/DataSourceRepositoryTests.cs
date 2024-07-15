using System.Collections.Immutable;
using AutoFixture;
using AutoMapper;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Moq.EntityFrameworkCore;
using ReData.Core;
using ReData.Database;
using ReData.Domain.Mapper;
using ReData.Domain.Repositories;
using ReData.Domain.Validators;
using Entity = ReData.Database.Entities;

namespace ReData.Domain.Tests;

public class DataSourceRepositoryTests
{
    
    private IRepository<Domain.DataSource> Repository()
    {
        var mappingConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile(new DataSourceMapping());
        });
        IMapper mapper = mappingConfig.CreateMapper();
        var appContextMock = new Mock<ApplicationDatabaseContext>();
        appContextMock.Setup<DbSet<Entity.DataSource>>
            (x => x.Set<Entity.DataSource>())
            .ReturnsDbSet(Array.Empty<Entity.DataSource>().AsQueryable().BuildMockDbSet().Object);
        appContextMock.Setup<DbSet<Entity.DataSourceParameter>>
                (x => x.Set<Entity.DataSourceParameter>())
            .ReturnsDbSet(Array.Empty<Entity.DataSourceParameter>().AsQueryable().BuildMockDbSet().Object);

        var rep = new DataSourceRepository
        {
            Database = appContextMock.Object,
            Mapper = mapper,
        };

        return new ValidatedRepository<DataSource>(rep)
        {
            Validator = new DataSourceValidator()
            {
                Database = appContextMock.Object,
            },
        };
    }
    
    [Fact]
    public async Task CannotCreateDataSourceWithUnknowType()
    {
        var sut = Repository();
        var input = new Fixture().Create<Domain.DataSource>() with
        {
            Type = Entity.DataSourceType.Unknown,
        };

        var result = await sut.CreateAsync(input, CancellationToken.None);

        result.Should().BeFailure();
    }
    
    [Fact]
    public async Task CannotCreateDataSourceWithNonexistentType()
    {
        var sut = Repository();
        var input = new Fixture().Create<Domain.DataSource>() with
        {
            Type = (Entity.DataSourceType) 55,
        };

        var result = await sut.CreateAsync(input, CancellationToken.None);

        result.Should().BeFailure();
    }
    
}