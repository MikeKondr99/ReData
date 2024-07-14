using AutoMapper;
using ReData.Application.Controllers;
using ReData.Domain;

namespace ReData.Application.Mapper;

public class DataSourceControllerMapperProfile : Profile
{
    public DataSourceControllerMapperProfile()
    {
        CreateMap<DataSource, DataSourceResponse>();
        CreateMap<CreateDataSource, DataSource>();
        CreateMap<UpdateDataSource, DataSource>();
    }
    
}