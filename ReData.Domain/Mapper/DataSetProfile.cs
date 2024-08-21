using AutoMapper;

namespace ReData.Domain.Mapper;

public sealed class DataSetProfile : Profile
{
    public DataSetProfile()
    {
        CreateMap<Entity.DataSet, Domain.DataSet>()
            .ForMember(d => d.Transformations, m => m.MapFrom(e => e.Transformations.Select(t => t.Data))
        );

        CreateMap<Domain.DataSet, Entity.DataSet>()
            .ForMember(e => e.Transformations,
                m => m.MapFrom(d => d.Transformations.Select((t, index) => new Entity.Transformation
                {
                    DataSetId = d.Id,
                    Order = (uint) index,
                    Data = t
                })
            )
        );

    }
    
}