using AutoMapper;
using FluentResults;
using ReData.Domain.Services.DataSource.Models;
using ReData.Domain.Services.QueryBuilder;
using ReData.Domain;
using DataSource = ReData.Domain.DataSource;

namespace ReData.Domain.Services;

public sealed class DataSourceService
{
    public required IRepository<Domain.DataSource> Repository { private get; init; }
    
    public required IMapper Mapper { private get; init; }
    
    public required IDataSourceConnectorFactory ConnectorFactory { private get; init; }

    public Task<Result<IEnumerable<Domain.DataSource>>> GetAsync(CancellationToken ct)
    {
        return Repository.GetAsync(x => true, ct);
    }
    
    public async Task<Result<Domain.DataSource?>> GetAsync(Guid id, CancellationToken ct)
    {
        return (await Repository.GetAsync(x => x.Id == id, ct)).Map(ds => ds.FirstOrDefault());
    }
    
    public async Task<Result<Domain.DataSource>> CreateAsync(CreateDataSource input, CancellationToken ct)
    {
        var dataSource = Mapper.Map<Domain.DataSource>(input);
        var connection = ConnectorFactory.CheckConnection(dataSource);
        
        if (connection.IsFailed) return connection.ToResult<Domain.DataSource>();

        var createResult = await Repository.CreateAsync(dataSource, ct);

        return createResult;
    }
    
    public async Task<Result<Domain.DataSource>> UpdateAsync(UpdateDataSource input, CancellationToken ct)
    {
        Result<Domain.DataSource> get = await Repository.GetByIdAsync(input.Id, ct);
        if (get.IsFailed) return get;
        
        var dataSource = Mapper.Map(input, get.Value);
        
        var connection = ConnectorFactory.CheckConnection(dataSource);
        
        if (connection.IsFailed) return connection.ToResult<Domain.DataSource>();
        
        var updateResult = await Repository.UpdateAsync(dataSource, ct);

        return updateResult;
    }
    
    public async Task<Result<Domain.DataSource>> DeleteAsync(Guid id, CancellationToken ct)
    {
        Result<Domain.DataSource> get = await Repository.GetByIdAsync(id, ct);
        if (get.IsFailed) return get;
        
        var deleteResult = await Repository.DeleteAsync(get.Value, ct);

        return deleteResult;
    }
}