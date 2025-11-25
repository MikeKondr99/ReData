namespace ReData.DemoApp.Tests;

public static class HttpResponseMessageExtensions
{
    public static async Task ShouldBeError(this Task<TestResult<ErrorResponse>> response, string key)
    {
        var (rsp, error) = await response;
        rsp.StatusCode.Should().Be(HttpStatusCode.BadRequest, because: "a validation error should return BadRequest status");
        error.Errors.Keys.Should().Contain("name", because: $"validation errors on {key} should be returned in the response body");
    }
    
    public static void ShouldBeError(this TestResult<ErrorResponse> response, string key)
    {
        var (rsp, error) = response;
        rsp.StatusCode.Should().Be(HttpStatusCode.BadRequest, because: "a validation error should return BadRequest status");
        error.Errors.Keys.Should().Contain(key, because: $"validation errors on {key} should be returned in the response body");
    }
}