using AutoMapper;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using ReData.Database;
using ReData.Database.Entities;

namespace ReData.Domain;

public abstract class Repository<T, TEntity> : IRepository<T>
    where TEntity : class
    where T : IEntity
{
    public required IDatabase Database { protected get; init; }

    public required IMapper Mapper { protected get; init; }

    protected abstract IQueryable<TEntity> Query(IQueryable<TEntity> query);

    protected virtual string EntityName { get; } = typeof(T).Name;

    public async Task<Result<IEnumerable<T>>> GetAsync(CancellationToken ct)
    {
        var entities = await Query(Database.Set<TEntity>().AsQueryable()).AsNoTracking().ToArrayAsync(ct);
        var result = Mapper.Map<IEnumerable<T>>(entities);
        return Result.Ok(result);
    }

    public async Task<Result<T>> GetAsync(Guid id, CancellationToken ct)
    {
        var entity = await Query(Database.Set<TEntity>().AsQueryable()).AsNoTracking().FirstOrDefaultAsync(ct);
        if (entity is null)
        {
            return Result.Fail($"Cannot get {EntityName}")
                .WithError($"{EntityName} with id:'{id}' not found");
        }

        return Result.Ok(Mapper.Map<T>(entity));
    }

    public async Task<Result<T>> CreateAsync(T entity, CancellationToken ct)
    {
        var obj = Mapper.Map<TEntity>(entity);
        await Database.Set<TEntity>().AddAsync(obj, ct);
        var save = await SaveChangesAsync(ct);
        if (save.IsFailed)
            return Result.Fail(OperationFail<T>.Create).WithReasons(save.Reasons);

        return Result.Ok(entity).WithReasons(save.Reasons);
    }

    public async Task<Result<T>> UpdateAsync(T entity, CancellationToken ct)
    {
        var obj = await Query(Database.Set<TEntity>().AsQueryable()).FirstOrDefaultAsync(ct);
        if (obj is null)
        {
            return Result
                .Fail(OperationFail<T>.Update)
                .WithError(EntityNotFound<T>.WithId(entity.Id));
        }

        Mapper.Map(entity, obj);
        var save = await SaveChangesAsync(ct);
        if (save.IsFailed)
            return Result.Fail(OperationFail<T>.Update).WithReasons(save.Reasons);
        return Result.Ok(entity).WithReasons(save.Reasons);
    }

    public async Task<Result<T>> DeleteAsync(T entity, CancellationToken ct)
    {
        var oldEntity = await Database.Set<TEntity>().FindAsync(entity.Id, ct);
        if (oldEntity is null)
        {
            return Result
                .Fail(OperationFail<T>.Delete)
                .WithError(EntityNotFound<T>.WithId(entity.Id));
        }
        Database.Set<TEntity>().Remove(oldEntity);
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