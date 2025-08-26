using ReData.DemoApplication.Database.Entities;
using ReData.DemoApplication.Repositories;
using ReData.DemoApplication.Transformations;

namespace ReData.DemoApplication.Mappers;

public static class DatasetMapper
{
    public static DataSetEntity ToEntity(this DataSet model)
    {
        return new DataSetEntity
        {
            Id = model.Id,
            Name = model.Name,
            Transformations = model.Transformations.Select((t, index) => new TransformationEntity
            {
                Enabled = t.Enabled,
                DataSetId = model.Id,
                Order = (uint)index,
                Data = t.Data
            }).ToList()
        };
    }
    
    

    public static DataSet ToModel(this DataSetEntity entity)
    {
        return new DataSet
        {
            Id = entity.Id,
            Name = entity.Name,
            Transformations = entity.Transformations.OrderBy(t => t.Order).Select(t => new Transformation()
            {
                Data = t.Data,
                Enabled = t.Enabled,
            }).ToArray(),
        };
    }
}