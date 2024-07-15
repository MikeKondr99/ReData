using System.Linq.Expressions;
using FluentResults;
using ReData.Database.Entities;

namespace ReData.Domain;

public interface IRepository<T> : IRepository<T,Guid>
where T : IEntity
{
}

public interface IRepository<T,TKey>
where T : IEntity<TKey>
{
    public Task<Result<IEnumerable<T>>> GetAsync(Func<T,bool> filter, CancellationToken ct = default);

    public Task<Result<T>> GetByIdAsync(TKey id, CancellationToken ct = default);

    public Task<Result<T>> CreateAsync(T entity, CancellationToken ct = default);

    public Task<Result<T>> UpdateAsync(T entity, CancellationToken ct = default);

    public Task<Result<T>> DeleteAsync(T entity, CancellationToken ct = default);
}
