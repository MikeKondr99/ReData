using Microsoft.EntityFrameworkCore;
using ReData.DemoApplication.Database;
using ReData.DemoApplication.Database.Entities;

namespace ReData.DemoApplication.Repositories;

public sealed class DataSetRepository : Repository<DataSet, DataSetEntity>
{
    public DataSetRepository(ApplicationDatabaseContext database)
        : base(database)
    {
    }

    protected override IQueryable<DataSetEntity> Query(IQueryable<DataSetEntity> query)
    {
        return Database.Set<DataSetEntity>().Include(ds => ds.Transformations);
    }

    protected override DataSetEntity ToEntity(DataSet model)
    {
        return new DataSetEntity
        {
            Id = model.Id,
            Name = model.Name,
            Transformations = model.Transformations.Select((t, index) => new TransformationEntity
            {
                DataSetId = model.Id,
                Order = (uint)index,
                Data = t
            }).ToArray()
        };
    }

    protected override DataSet FromEntity(DataSetEntity entity)
    {
        return new DataSet
        {
            Id = entity.Id,
            Name = entity.Name,
            Transformations = entity.Transformations.OrderBy(t => t.Order).Select(t => t.Data).ToArray(),
        };
    }
}