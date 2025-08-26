using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pattern.Unions;
using ReData.DemoApplication.Database;
using ReData.DemoApplication.Database.Entities;
using ReData.DemoApplication.Mappers;
using ReData.DemoApplication.Repositories;
using ReData.DemoApplication.Requests;

namespace ReData.DemoApplication.Controllers;

[ApiController]
[Route("api/datasets")]
public class DataSetController : ControllerBase
{
    private readonly IRepository<DataSet> repository;
    private readonly ApplicationDatabaseContext context;

    public DataSetController(IRepository<DataSet> repository, ApplicationDatabaseContext context)
    {
        this.repository = repository;
        this.context = context;
    }

    // GET: api/datasets
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DataSet>>> GetAll(CancellationToken ct = default)
    {
        var result = await repository.GetAsync(ct: ct);

        return result.Match(
            datasets => Ok(datasets.Value),
            error => Problem(detail: error.Value, statusCode: StatusCodes.Status500InternalServerError)
        );
    }

    // GET: api/datasets/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DataSet>> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await repository.GetByIdAsync(id, ct);

        if (result.Unwrap(out var ok, out var err))
        {
            return Ok(ok);
        }

        return BadRequest(err);
    }

    // POST: api/datasets
    [HttpPost]
    public async Task<ActionResult<DataSet>> Create([FromBody] CreateDataSetRequest request, CancellationToken ct = default)
    {
        var dataset = new DataSet()
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Transformations = request.Transformations,
        };
            
        var result = await repository.CreateAsync(dataset, ct);

        if (result.Unwrap(out var ok, out var err))
        {
            var routeValues = new
            {
                id = ok.Id
            };
            return CreatedAtAction(nameof(GetById), routeValues, ok);
        }

        return BadRequest(err);
    }

// PUT: api/datasets/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DataSet>> Update(Guid id, [FromBody] DataSet dataset, CancellationToken ct = default)
    {
        if (id != dataset.Id)
        {
            return BadRequest("ID mismatch");
        }

        var entity = await context.DataSets
            .Include(ds => ds.Transformations)
            .FirstOrDefaultAsync(ds => ds.Id == id, ct);

        if (entity is null)
        {
            return NotFound();
        }

        // Update simple properties
        entity.Name = dataset.Name;
        
        // Remove existing transformations from context tracking FIRST
        context.Set<TransformationEntity>().RemoveRange(entity.Transformations);
    
        // Clear existing transformations and add new ones
        entity.Transformations.Clear();
        for (int i = 0; i < dataset.Transformations.Count; i++)
        {
            entity.Transformations.Add(new TransformationEntity
            {
                Enabled = dataset.Transformations[i].Enabled,
                DataSetId = dataset.Id,
                Order = (uint)i,
                Data = dataset.Transformations[i].Data,
            });
        }

        await context.SaveChangesAsync(ct);
    
        return dataset;
    }
    // DELETE: api/datasets/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var result = await repository.DeleteAsync(id, ct);

        if (result.Unwrap(out var ok, out var err))
        {
            return Ok(ok);
        }
        return BadRequest(err);
    }
}