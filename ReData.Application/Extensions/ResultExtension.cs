using System.Text.Json.Serialization;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using ReData.Domain;

namespace ReData.Application;

public static class ResultExtension
{
    public static ActionResult<T> ToResponse<T>(this Result<T> result) 
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(result.Value);
        }


        return ToErrorResponse(result);
    }
    
    public static ActionResult ToErrorResponse(this IResultBase result)
    {
        var error = result.Errors.FirstOrDefault();
        var obj = new ErrorObject() { Errors = result.Errors.Select(e => new Error(e)) };
        
        if (error is IServiceUnavailableError)
        {
            return new ObjectResult(obj)
            {
                StatusCode = StatusCodes.Status503ServiceUnavailable
            };
        }
        
        if (error is INotFoundError)
        {
            return new NotFoundObjectResult(obj);
        }

        if (error is IUserError)
        {
            return new BadRequestObjectResult(obj);
        }
        
        return new ObjectResult(obj)
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };
        
    }
    
}

public record ErrorObject
{
    public required IEnumerable<Error> Errors { get; init; }
}

public class Error(IError innerError)
{
    public string Message => innerError.Message;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Metadata => innerError.Metadata?.Any() == true  ? innerError.Metadata : null;
}
