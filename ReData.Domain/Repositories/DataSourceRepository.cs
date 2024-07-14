using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ReData.Database;
using ReData.Database.Entities;

namespace ReData.Domain.Repositories;

public sealed class DataSourceRepository : Repository<DataSource,Database.Entities.DataSource>
{


    protected override IQueryable<Database.Entities.DataSource> Query(IQueryable<Database.Entities.DataSource> query)
    {
        return query.Include(ds => ds.Parameters);
    }
}
