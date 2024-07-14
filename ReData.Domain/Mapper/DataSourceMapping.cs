using AutoMapper;
using ReData.Database.Entities;

namespace ReData.Domain.Mapper;

public class DataSourceMapping : Profile
{
    public DataSourceMapping()
    {
        CreateMap<Database.Entities.DataSource, DataSource>().ForMember(ds => ds.Parameters,
            opt => opt.MapFrom(ds => ds.Parameters.ToDictionary(p => p.Key, p => p.Value)));

        CreateMap<DataSource, Database.Entities.DataSource>()
            .ForMember(ds => ds.Parameters, o => o.MapFrom(ds => ds.Parameters.Select(kv => new DataSourceParameter
            {
                DataSourceId = ds.Id,
                Key = kv.Key,
                Value = kv.Value,
            })));
    }
}