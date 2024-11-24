using FluentResults;
using FluentValidation;
using ReData.Database.Entities;

namespace ReData.Domain.Repositories;

public sealed class ValidatedRepository<T> : IRepository<T> 
where T : IEntity
{
    public ValidatedRepository(IRepository<T> inner)
    {
        InnerRepository = inner;
    }
    public required IValidator<T> Validator { private get; init; }
    
    private IRepository<T> InnerRepository { get; init; }

    private async Task<Result<T>> ValidateAnd(T entity, CancellationToken ct, Func<T,CancellationToken,Task<Result<T>>> func)
    {
        var validationResult = await Validator.ValidateAsync(entity, ct);
        if (!validationResult.IsValid)
            return Result.Fail(new EntityNotValid<T>(validationResult));
        return await func(entity, ct);
    }
    
    
    public Task<Result<IEnumerable<T>>> GetAsync(Func<T,bool>? filter = null, CancellationToken ct = default)
    {
        return InnerRepository.GetAsync(filter, ct);
    }

    public Task<Result<T>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return InnerRepository.GetByIdAsync(id, ct);
    }

    public async Task<Result<T>> CreateAsync(T entity, CancellationToken ct = default)
    {
        return await ValidateAnd(entity, ct, InnerRepository.CreateAsync);
    }

    public async Task<Result<T>> UpdateAsync(T entity, CancellationToken ct = default)
    {
        return await ValidateAnd(entity, ct, InnerRepository.UpdateAsync);
    }

    public Task<Result<T>> DeleteAsync(T entity, CancellationToken ct = default)
    {
        return InnerRepository.DeleteAsync(entity, ct);
    }
}