using Microsoft.AspNetCore.Mvc;
using Pattern.Unions;
using ReData.DemoApplication.Repositories;

namespace ReData.DemoApplication.Controllers;

[ApiController]
[Route("api/datasets")]
public class DataSetController : ControllerBase
{
    private readonly IRepository<DataSet> repository;

    public DataSetController(IRepository<DataSet> repository)
    {
        this.repository = repository;
    }

    // GET: api/datasets
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DataSet>>> GetAll(CancellationToken ct = default)
    {
        var result = await repository.GetAsync(ct: ct);

        return result.Match(
            datasets => Ok(datasets),
            error => Problem(detail: error, statusCode: StatusCodes.Status500InternalServerError)
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
    public async Task<ActionResult<DataSet>> Create([FromBody] DataSet dataset, CancellationToken ct = default)
    {
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

        var result = await repository.UpdateAsync(dataset, ct);

        if (result.Unwrap(out var ok, out var err))
        {
            return Ok(ok);
        }

        return BadRequest(err);
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