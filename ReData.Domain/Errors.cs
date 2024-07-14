using FluentResults;

namespace ReData.Domain;


public class OperationFail<T> : Error
{
    private OperationFail() { }
    
    public static OperationFail<T> Update => new OperationFail<T> {  Message = $"{typeof(T).Name} update failed"};
    public static OperationFail<T> Create => new OperationFail<T> {  Message = $"{typeof(T).Name} creation failed"};
    public static OperationFail<T> Delete => new OperationFail<T> {  Message = $"{typeof(T).Name} deletion failed"};
}

public class EntityNotFound<T> : Error
{
    private EntityNotFound() { }
    private Guid Id { get; init; }

    public static EntityNotFound<T> WithId(Guid id) => new() { Message = $"{typeof(T).Name} with id:'{id}' not found" };

}