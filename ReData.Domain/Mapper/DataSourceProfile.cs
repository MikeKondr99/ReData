using System.Runtime.InteropServices.ComTypes;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ReData.Core;
using ReData.Database.Entities;
using ReData.Domain.Services.DataSource.Models;

namespace ReData.Domain.Mapper;

public class DataSourceProfile : Profile
{
    public DataSourceProfile()
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

        CreateMap<CreateDataSource, Domain.DataSource>();
        CreateMap<UpdateDataSource, Domain.DataSource>()
            .ForMember(ds => ds.Id, opt => opt.Ignore())
            .ForMember(ds => ds.Parameters, opt => opt.MapFrom((u, ds) =>
            {
                foreach (var p in u.Parameters)
                {
                    if (p.Value is null)
                    {
                        ds.Parameters.Remove(p.Key);
                    }
                    else
                    {
                        ds.Parameters[p.Key] = p.Value;
                    }
                }
                return new Dictionary<StringKey,string>(ds.Parameters);
            }));
    }
}