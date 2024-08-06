using FluentResults;
using FluentValidation.Results;

namespace ReData.Domain;


public interface IUserError;

public interface IServerError;

public interface IServiceUnavailableError : IServerError;

public interface INotFoundError : IUserError;

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

public class EntityNotValid<T> : Error, IUserError
{
    private static string ValidationError = $"Validation of {typeof(T).Name} failed";
    public EntityNotValid(ValidationResult validationResult)
    {
        Message = ValidationError;
        foreach (var error in validationResult.Errors.ToLookup(x => x.PropertyName))
        {
            
            Metadata[error.Key] = error.Select(x => x.ErrorMessage).ToArray();
        }
    }
    
    

}

public class ConnectionInvalid : Error, IUserError
{
    public ConnectionInvalid(Dictionary<string,string> parameters)
    {
        Message = "Error in connection parameters";
        foreach (var p in parameters)
        {
            Metadata[p.Key] = p.Value;
        }
        
    }
}

public class ConnectionAuthFailed : Error, IUserError
{
    public ConnectionAuthFailed()
    {
        Message = "Authentication failed";
    }
}

public class ConnectionCrushed : Error, IServiceUnavailableError
{
    public ConnectionCrushed(string message)
    {
        Message = message;
    }
    
}
