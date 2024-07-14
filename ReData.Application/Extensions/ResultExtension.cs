using System.Text.Json.Serialization;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

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
        
        return new BadRequestObjectResult(new ErrorObject()
        {
            Errors = result.Errors.Select(e => new Error(e))
        });
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
