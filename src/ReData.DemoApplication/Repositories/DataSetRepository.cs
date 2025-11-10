using Microsoft.EntityFrameworkCore;
using ReData.DemoApplication.Database;
using ReData.DemoApplication.Database.Entities;
using ReData.DemoApplication.Transformations;

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
                Enabled = t.Enabled,
                Description = t.Description,
                DataSetId = model.Id,
                Order = (uint)index,
                Data = t.Transformation
            }).ToList()
        };
    }

    protected override DataSet FromEntity(DataSetEntity entity)
    {
        return new DataSet
        {
            Id = entity.Id,
            Name = entity.Name,
            Transformations = entity.Transformations.OrderBy(t => t.Order).Select(t => new TransformationBlock()
            {
                Transformation = t.Data,
                Description = t.Description,
                Enabled = t.Enabled,
            }).ToArray(),
        };
    }

}