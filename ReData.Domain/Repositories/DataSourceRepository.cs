using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ReData.Database;
using ReData.Database.Entities;

namespace ReData.Domain.Repositories;

public sealed class DataSourceRepository : Repository<DataSource,Entity.DataSource>
{
    protected override IQueryable<Entity.DataSource> Query(IQueryable<Entity.DataSource> query)
    {
        return query.Include(ds => ds.Parameters);
    }
}

public sealed class DataSetRepository : Repository<DataSet, Entity.DataSet>
{
    protected override IQueryable<Entity.DataSet> Query(IQueryable<Entity.DataSet> query)
    {
        return query;
    }
}
