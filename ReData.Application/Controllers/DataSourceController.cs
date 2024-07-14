using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ReData.Database.Entities;
using ReData.Domain;
using ReData.Domain.Repositories;
using DataSource = ReData.Domain.DataSource;

namespace ReData.Application.Controllers;

public record DataSourceResponse
{
    public required Guid Id { get; init; }
    
    public required DataSourceType Type { get; init; }

    public required string Name { get; init; }

    public string? Description { get; init; }

    public required Dictionary<string, string> Parameters { get; init; }
}

public record CreateDataSource
{
    public required string Name { get; init; }
    
    public required string? Description { get; init; }
    
    public required DataSourceType Type { get; init; }
    
    public required Dictionary<string, string> Parameters { get; init; }
}

public record UpdateDataSource
{
    public required string Name { get; init; }
    
    public required string? Description { get; init; }
    
    public required Dictionary<string, string> Parameters { get; init; }
}

[ApiController]
[Route("/api/datasource")]
public class DataSourceController : Controller
{
    public required IRepository<DataSource> Repository { private get; init; }
    
    public required IMapper Mapper { private get; init; }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DataSourceResponse>>> GetAllDataSources(CancellationToken ct)
    {
        Result<IEnumerable<DataSource>> result = await Repository.GetAsync(ct);
        if (result.IsFailed)
        {
            BadRequest(result.Errors);
        }
        return Ok(Mapper.Map<IEnumerable<DataSourceResponse>>(result.Value));
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<DataSourceResponse>> GetDataSourceById(Guid id, CancellationToken ct)
    {
        Result<DataSource> result = await Repository.GetAsync(id,ct);
        if (result.IsFailed)
        {
            BadRequest(result.Errors);
        }
        return Ok(Mapper.Map<DataSourceResponse>(result.Value));
    }
    
    [HttpPut("{id}")]
    public async Task<ActionResult<DataSourceResponse>> UpdateDataSource([FromRoute] Guid id, [FromBody] UpdateDataSource body, CancellationToken ct)
    {
        Result<DataSource> get = await Repository.GetAsync(id, ct);
        if (get.IsFailed) return get.ToErrorResponse();

        Mapper.Map(body, get.Value);

        Result<DataSource> result = await Repository.UpdateAsync(get.Value, ct);
        if (get.IsFailed) return result.ToErrorResponse();

        return Ok(Mapper.Map<DataSourceResponse>(result.Value));
    }
    
    [HttpPost]
    public async Task<ActionResult<DataSourceResponse>> CreateDataSource([FromBody] CreateDataSource body, CancellationToken ct)
    {
        var input = Mapper.Map<DataSource>(body);
        
        var result = await Repository.CreateAsync(input, ct);
        if (result.IsFailed) return result.ToErrorResponse();
        
        return Ok(Mapper.Map<DataSourceResponse>(result.Value));
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult<DataSourceResponse>> DeleteDataSource([FromRoute] Guid id, CancellationToken ct)
    {
        Result<DataSource> get = await Repository.GetAsync(id, ct);
        if (get.IsFailed) BadRequest(get.Errors);
        
        var result = await Repository.DeleteAsync(get.Value, ct);
        if (result.IsFailed) return BadRequest(result.Errors);
        
        return Ok(Mapper.Map<DataSourceResponse>(result.Value));
    }
}