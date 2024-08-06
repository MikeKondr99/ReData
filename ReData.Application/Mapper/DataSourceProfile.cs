using AutoMapper;
using ReData.Application.Controllers.DataSource;
using ReData.Domain.Services.DataSource.Models;

namespace ReData.Application.Mapper;

public class DataSourceProfile : Profile
{
    public DataSourceProfile()
    {
        CreateMap<CreateDataSourceRequest, CreateDataSource>();
        CreateMap<UpdateDataSourceRequest, UpdateDataSource>();
        CreateMap<Domain.DataSource, DataSourceResponse>();
        CreateMap<Domain.DataSource, DataSourceWithParametersResponse>();
    }
    
}