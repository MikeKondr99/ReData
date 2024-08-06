using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ReData.Domain.Services;
using ReData.Domain.Services.DataSource.Models;

namespace ReData.Application.Controllers.DataSource;

[ApiController]
[Route("/api/datasource")]
public class DataSourceController : Controller
{
    
    public required DataSourceService Service { private get; init; }
    
    public required IMapper Mapper { private get; init; }
    
    /// <summary>
    /// Создать источник данных
    /// </summary>
    /// <param name="body"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<DataSourceWithParametersResponse>> Create([FromBody] CreateDataSourceRequest body, CancellationToken ct)
    {
        CreateDataSource input = Mapper.Map<CreateDataSource>(body);
        var result = await Service.CreateAsync(input, ct);
        return result.Map(x => Mapper.Map<DataSourceWithParametersResponse>(x)).ToResponse();
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DataSourceWithParametersResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateDataSourceRequest body,
        CancellationToken ct)
    {
        UpdateDataSource input = Mapper.Map<UpdateDataSource>(body) with { Id = id };
        var result = await Service.UpdateAsync(input, ct);
        return result.Map(x => Mapper.Map<DataSourceWithParametersResponse>(x)).ToResponse();
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<DataSourceResponse>> Delete(Guid id,
        CancellationToken ct)
    {
        var result = await Service.DeleteAsync(id, ct);
        return result.Map(x => Mapper.Map<DataSourceResponse>(x)).ToResponse();
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DataSourceResponse>>> List(CancellationToken ct)
    {
        var result = await Service.GetAsync(ct);
        return result.Map(x => Mapper.Map<IEnumerable<DataSourceResponse>>(x)).ToResponse();
    }
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DataSourceWithParametersResponse>> One([FromRoute] Guid id,CancellationToken ct)
    {
        var result = await Service.GetAsync(id,ct);
        return result.Map(x => Mapper.Map<DataSourceWithParametersResponse>(x)).ToResponse();
    }
    
}