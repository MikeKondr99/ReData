using Pattern.Unions;

namespace ReData.DemoApplication.Repositories;

public interface IEntity<TKey>
{
    public TKey Id { get; }
}

public interface IEntity : IEntity<Guid>;

public interface IRepository<T> : IRepository<T, Guid>
    where T : IEntity
{
}

public interface IRepository<T, in TKey>
    where T : IEntity<TKey>
{
    public Task<Result<IEnumerable<T>, string>> GetAsync(CancellationToken ct = default);

    public Task<Result<T, string>> GetByIdAsync(TKey id, CancellationToken ct = default);

    public Task<Result<T, string>> CreateAsync(T entity, CancellationToken ct = default);

    public Task<Result<T, string>> UpdateAsync(T model, CancellationToken ct = default);

    public Task<Result<T, string>> DeleteAsync(TKey id, CancellationToken ct = default);
    
    public Task<Result<int, string>> SaveChangesAsync(CancellationToken ct = default);

    public Task<Result<T, string>> DeleteAsync(T entity, CancellationToken ct = default)
    {
        return DeleteAsync(entity.Id, ct);
    }

}