using Microsoft.EntityFrameworkCore;
using ReData.Database;
using ReData.Database.Entities;
using ReData.DemoApplication;

namespace ReData.DemoApplication;

public class DataSetsService
{
    private readonly ApplicationDatabaseContext _context;
    
    private readonly ILogger<DataSetsService> _logger;

    public DataSetsService(ApplicationDatabaseContext context, ILogger<DataSetsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Create
    public async Task<DataSet> CreateDataSetAsync(DataSet dataSet, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = MapToEntity(dataSet);
            
            await _context.DataSets.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Created new DataSet with ID {DataSetId}", dataSet.Id);
            
            return MapToModel(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating DataSet");
            throw;
        }
    }

    // Read (single)
    public async Task<DataSet?> GetDataSetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _context.DataSets
                .Include(ds => ds.Transformations)
                .FirstOrDefaultAsync(ds => ds.Id == id, cancellationToken);

            return entity != null ? MapToModel(entity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving DataSet with ID {DataSetId}", id);
            throw;
        }
    }

    // Read (all)
    public async Task<IEnumerable<DataSet>> GetAllDataSetsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _context.DataSets
                .ToListAsync(cancellationToken);

            return entities.Select(MapToModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all DataSets");
            throw;
        }
    }

    // Update
    public async Task<DataSet> UpdateDataSetAsync(DataSet dataSet, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingEntity = await _context.DataSets
                .Include(ds => ds.Transformations)
                .AsNoTracking()
                .FirstOrDefaultAsync(ds => ds.Id == dataSet.Id, cancellationToken);

            if (existingEntity == null)
            {
                throw new KeyNotFoundException($"DataSet with ID {dataSet.Id} not found");
            }
            
            existingEntity = MapToEntity(dataSet);
            _context.Update(existingEntity);

            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Updated DataSet with ID {DataSetId}", dataSet.Id);
            
            return MapToModel(existingEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating DataSet with ID {DataSetId}", dataSet.Id);
            throw;
        }
    }

    // Delete
    public async Task DeleteDataSetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _context.DataSets.FindAsync(new object[] { id }, cancellationToken);
            
            if (entity == null)
            {
                throw new KeyNotFoundException($"DataSet with ID {id} not found");
            }

            _context.DataSets.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Deleted DataSet with ID {DataSetId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting DataSet with ID {DataSetId}", id);
            throw;
        }
    }

    // Mapping methods
    private static DataSetEntity MapToEntity(DataSet model)
    {
        return new DataSetEntity
        {
            Id = model.Id,
            Name = model.Name,
            Transformations = model.Transformations
                .Select((t, i) => new TransformationEntity
                {
                    DataSetId = model.Id,
                    Order = (uint)i,
                    Data = t
                })
                .ToList()
        };
    }

    private static DataSet MapToModel(DataSetEntity entity)
    {
        return new DataSet
        {
            Id = entity.Id,
            Name = entity.Name,
            Transformations = entity.Transformations
                .OrderBy(t => t.Order)
                .Select(t => t.Data)
                .ToList()
        };
    }
}
    
    
    

public sealed record DataSet
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required ICollection<ITransformation> Transformations { get; init; }
}
