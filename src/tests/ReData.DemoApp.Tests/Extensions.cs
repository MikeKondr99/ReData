using FastEndpoints;
using ReData.DemoApp.Transformations;
using TUnit.Assertions.Sources;


public static class Extensions 
{
    
    public async static Task IsValidationError(this ValueAssertion<TestResult<ErrorResponse>> assertion, string key)
    {
        await assertion.Member(r => r.Response.StatusCode, rsp => rsp.IsEqualTo(HttpStatusCode.BadRequest).Because("a validation error should return BadRequest status"))
            .And.Member(r => r.Result.Errors.Keys, errorKeys => errorKeys.Contains(key).Because($"validation errors on {key} should be returned in the response body"));
    }
    
    public async static Task IsValidationError(this ValueAssertion<TestResult<ErrorResponse>> assertion)
    {
        await assertion.Member(r => r.Response.StatusCode, rsp => rsp.IsEqualTo(HttpStatusCode.BadRequest).Because("a validation error should return BadRequest status"));
    }
    
    public static async Task<T> IsSuccess<T>(this Task<TestResult<T>> response)
    {
        var (rsp, body) = await response;
        await Assert.That((int)rsp.StatusCode).IsBetween(200, 299);
        return body;
    }
    
    public static TransformationBlock Block(this Transformation transformation, bool enabled = true)
    {
        return new TransformationBlock()
        {
            Enabled = enabled,
            Transformation = transformation,
        };
    }

    public static SelectItem As(this string expression, string alias)
    {
        return new SelectItem()
        {
            Field = alias,
            Expression = expression,
        };
    }
    
    public static OrderItem Asc(this string expression)
    {
        return new OrderItem()
        {
            Expression = expression,
            Descending = false,
        };
    }
    
    public static OrderItem Desc(this string expression)
    {
        return new OrderItem()
        {
            Expression = expression,
            Descending = true,
        };
    }
    
    
}
