using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pattern.Unions;
using ReData.DemoApplication.Database;

namespace ReData.DemoApplication.Repositories;

public abstract class Repository<T, TEntity> : IRepository<T>
    where TEntity : class, IEntity
    where T : IEntity
{
    public Repository(ApplicationDatabaseContext database)
    {
        Database = database;
    }

    protected ApplicationDatabaseContext Database { get; }

    protected abstract IQueryable<TEntity> Query(IQueryable<TEntity> query);

    protected abstract TEntity ToEntity(T model);

    protected abstract T FromEntity(TEntity entity);

    private static string EntityName { get; } = typeof(T).Name;

    private async Task<Result<TEntity, string>> GetEntityByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await Query(Database.Set<TEntity>().AsQueryable()).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null)
        {
            return $"{EntityName} with id:{id} not found";
        }

        return entity;
    }

    public async Task<Result<T, string>> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return (await GetEntityByIdAsync(id, ct)).Map(x => FromEntity(x));
    }

    public async Task<Result<T, string>> CreateAsync(T model, CancellationToken ct)
    {
        var entity = ToEntity(model);
        await Database.Set<TEntity>().AddAsync(entity, ct);
        var save = await SaveChangesAsync(ct);

        return save.Map(_ => model);
    }

    public async Task<Result<T, string>> UpdateAsync(T model, CancellationToken ct)
    {
        var get = await GetEntityByIdAsync(model.Id, ct);
        if (get.UnwrapErr(out var error, out var entity))
        {
            return error;
        }

        Database.Entry(entity).CurrentValues.SetValues(ToEntity(model));

        var save = await SaveChangesAsync(ct);

        return save.Map(_ => model);
    }

    public async Task<Result<T, string>> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var get = await GetEntityByIdAsync(id, ct);
        if (get.UnwrapErr(out var error, out var entity))
        {
            return error;
        }

        Database.Set<TEntity>().Remove(entity);

        var save = await SaveChangesAsync(ct);

        return save.Map(_ => FromEntity(entity));
    }

    Task<Result<IEnumerable<T>, string>> IRepository<T, Guid>.GetAsync(Func<T, bool>? filter, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<int, string>> SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            var save = await Database.SaveChangesAsync(ct);
            return save;
        }
        catch (Exception ex)
        {
            if (ex.InnerException is PostgresException pg)
            {
                if (pg.SqlState == "23505")
                {
                    return "Unique constraint violation";
                }
            }

            return ex.Message;
        }
    }
}