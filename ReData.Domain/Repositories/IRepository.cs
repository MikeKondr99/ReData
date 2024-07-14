using FluentResults;

namespace ReData.Domain;

public interface IRepository<T>
{
    public Task<Result<IEnumerable<T>>> GetAsync(CancellationToken ct);

    public Task<Result<T>> GetAsync(Guid id, CancellationToken ct);

    public Task<Result<T>> CreateAsync(T entity, CancellationToken ct);

    public Task<Result<T>> UpdateAsync(T entity, CancellationToken ct);

    public Task<Result<T>> DeleteAsync(T entity, CancellationToken ct);
}