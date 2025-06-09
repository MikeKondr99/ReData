using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using ReData.DemoApplication.Responses;
using ReData.Query.Impl.Functions;

namespace ReData.DemoApplication.Controllers;

[ApiController]
[Route("api/functions")]
public sealed class FunctionsController : ControllerBase
{
    public IEnumerable<FunctionViewModel> Get()
    {
        var functions = GlobalFunctionsStorage.Functions
            .Where(f => f.Templates.Keys.Any(k => k.HasFlag(DatabaseTypes.PostgreSql)))
            .Where(f => f.ImplicitCast is null)
            .OrderBy(f => f.Name);


        return functions.Select(f => new FunctionViewModel()
        {
            Name = f.Name,
            Arguments = f.Arguments,
            Doc = f.Doc,
            Kind = f.Kind,
            ReturnType = f.ReturnType,
            DisplayText = f.ToString(),
        });
        
    }
        
    
}