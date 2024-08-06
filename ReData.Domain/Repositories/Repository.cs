using System.Linq.Expressions;
using AutoMapper;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using ReData.Database;
using ReData.Database.Entities;

namespace ReData.Domain;

public abstract class Repository<T, TEntity> : IRepository<T>
    where TEntity : class, IEntity
    where T : IEntity
{
    public required IDatabase Database { protected get; init; }

    public required IMapper Mapper { protected get; init; }

    protected abstract IQueryable<TEntity> Query(IQueryable<TEntity> query);

    private static string EntityName { get; } = typeof(T).Name;

    public async Task<Result<IEnumerable<T>>> GetAsync(Func<T,bool>? filter, CancellationToken ct = default)
    {
        filter ??= x => true;
        var entities = await Query(Database.Set<TEntity>()).AsNoTracking().ToListAsync(ct);
        var result = Mapper.Map<IEnumerable<T>>(entities).Where(filter);
        
        return Result.Ok(result);
    }

    public async Task<Result<T>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return (await GetEntityByIdAsync(id, ct)).Map(x => Mapper.Map<T>(x));
    }
    
    private async Task<Result<TEntity>> GetEntityByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await Query(Database.Set<TEntity>().AsQueryable()).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id,ct);
        if (entity is null)
        {
            return Result.Fail($"Cannot get {EntityName}")
                .WithError($"{EntityName} with id:'{id}' not found");
        }
        return Result.Ok(entity);
    }

    public async Task<Result<T>> CreateAsync(T entity, CancellationToken ct = default)
    {
        var obj = Mapper.Map<TEntity>(entity);
        await Database.Set<TEntity>().AddAsync(obj, ct);
        var save = await SaveChangesAsync(ct);
        if (save.IsFailed)
            return Result.Fail(OperationFail<T>.Create).WithReasons(save.Reasons);

        return Result.Ok(entity).WithReasons(save.Reasons);
    }

    public async Task<Result<T>> UpdateAsync(T entity, CancellationToken ct = default)
    {
        var obj = await GetEntityByIdAsync(entity.Id, ct);
        if (obj.IsFailed) return obj.ToResult();

        Mapper.Map(entity, obj.Value);
        
        var save = await SaveChangesAsync(ct);
        if (save.IsFailed)
            return Result.Fail(OperationFail<T>.Update).WithReasons(save.Reasons);
        return Result.Ok(entity).WithReasons(save.Reasons);
    }

    public async Task<Result<T>> DeleteAsync(T entity, CancellationToken ct = default)
    {
        var obj = await GetEntityByIdAsync(entity.Id, ct);
        if (obj.IsFailed) return obj.ToResult();
        
        Database.Set<TEntity>().Remove(obj.Value);
        var save = await SaveChangesAsync(ct);
        if (save.IsFailed)
            return Result.Fail(OperationFail<T>.Delete).WithReasons(save.Reasons);
        return Result.Ok(entity).WithReasons(save.Reasons);
    }

    public async Task<Result> SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            var save = await Database.SaveChangesAsync(ct);
            return Result.Ok().WithSuccess($"{save} entities was updated");
        }
        catch (Exception ex)
        {
            if (ex.InnerException is PostgresException pg)
            {
                if (pg.SqlState == "23505")
                {
                    return Result
                        .Fail(new Error("Unique constraint violation")
                            .WithMetadata(pg?.ConstraintName?.Split('_').LastOrDefault(), "Must have unique value"));
                }
            }

            return Result.Fail(ex.Message);
        }
    }
}