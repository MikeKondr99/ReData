using AutoMapper;
using FluentResults;
using ReData.Domain.Services.DataSet.Models;
using ReData.Domain.Services.DataSource.Models;
using ReData.Domain.Services.QueryBuilder;

namespace ReData.Domain.Services.DataSet;

public sealed class DataSourceService
{
    public required IRepository<Domain.DataSet> Repository { private get; init; }
    
    public required IMapper Mapper { private get; init; }
    
    public required IDataSourceConnectorFactory ConnectorFactory { private get; init; }

    public Task<Result<IEnumerable<Domain.DataSet>>> GetAsync(CancellationToken ct)
    {
        return Repository.GetAsync(x => true, ct);
    }
    
    public async Task<Result<Domain.DataSet?>> GetAsync(Guid id, CancellationToken ct)
    {
        return (await Repository.GetAsync(x => x.Id == id, ct)).Map(ds => ds.FirstOrDefault());
    }
    
    public async Task<Result<Domain.DataSet>> CreateAsync(CreateDataSet input, CancellationToken ct)
    {
        var dataSet = Mapper.Map<Domain.DataSet>(input);

        var createResult = await Repository.CreateAsync(dataSet, ct);

        return createResult;
    }
    
    public async Task<Result<Domain.DataSet>> UpdateAsync(UpdateDataSet input, CancellationToken ct)
    {
        Result<Domain.DataSet> get = await Repository.GetByIdAsync(input.Id, ct);
        if (get.IsFailed) return get;
        
        var dataSet = Mapper.Map(input, get.Value);
        
        var updateResult = await Repository.UpdateAsync(dataSet, ct);

        return updateResult;
    }
    
    public async Task<Result<Domain.DataSet>> DeleteAsync(Guid id, CancellationToken ct)
    {
        Result<Domain.DataSet> get = await Repository.GetByIdAsync(id, ct);
        if (get.IsFailed) return get;
        
        var deleteResult = await Repository.DeleteAsync(get.Value, ct);

        return deleteResult;
    }
}
