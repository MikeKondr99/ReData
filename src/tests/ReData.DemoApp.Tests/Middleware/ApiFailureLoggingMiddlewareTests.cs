using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using ReData.DemoApp.Middleware;
using TUnit.Assertions.Exceptions;

namespace ReData.DemoApp.Tests.Middleware;

public class ApiFailureLoggingMiddlewareTests
{
    [Test]
    public async Task InvokeApiRequest_With400Status_AddsMessageAndValidationErrorTags()
    {
        // Arrange
        var middleware = new ApiFailureLoggingMiddleware(
            async context =>
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    """{"statusCode":400,"message":"One or more errors occurred!","errors":{"name":["'name' should be at least 3 characters."]}}""");
            });

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/datasets/";
        context.Request.Method = HttpMethods.Post;
        context.Response.Body = new MemoryStream();

        using var activity = new Activity("http-request");
        activity.Start();

        // Act
        await middleware.Invoke(context);

        // Assert
        await Assert.That(activity.Status).IsEqualTo(ActivityStatusCode.Error);
        await Assert.That(activity.StatusDescription).IsEqualTo("One or more errors occurred!");
        await Assert.That(activity.Tags.Any(t => t.Key == "redata.error.message" && t.Value == "One or more errors occurred!")).IsTrue();
        await Assert.That(activity.Tags.Any(t => t.Key == "errors.name" && t.Value == "'name' should be at least 3 characters.")).IsTrue();
    }

    [Test]
    public async Task InvokeApiRequest_WhenExceptionThrown_AddsExceptionTagsAndRethrows()
    {
        // Arrange
        var middleware = new ApiFailureLoggingMiddleware(
            _ => throw new InvalidOperationException("boom"));

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/fail";
        context.Request.Method = HttpMethods.Get;
        context.Response.Body = new MemoryStream();

        using var activity = new Activity("http-request");
        activity.Start();

        // Act
        // Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await middleware.Invoke(context));
        await Assert.That(ex).IsNotNull();
        await Assert.That(ex!.Message).IsEqualTo("boom");
        await Assert.That(activity.Status).IsEqualTo(ActivityStatusCode.Error);
        await Assert.That(activity.StatusDescription).IsEqualTo("boom");
        await Assert.That(activity.Tags.Any(t => t.Key == "exception.message" && t.Value == "boom")).IsTrue();
        await Assert.That(activity.Tags.Any(t => t.Key == "exception.type" &&
                                                 t.Value is not null &&
                                                 t.Value.Contains("InvalidOperationException", StringComparison.Ordinal))).IsTrue();
    }

    [Test]
    public async Task InvokeNonApiRequest_DoesNotSetErrorStatus()
    {
        // Arrange
        var middleware = new ApiFailureLoggingMiddleware(
            context =>
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                return Task.CompletedTask;
            });

        var context = new DefaultHttpContext();
        context.Request.Path = "/health";
        context.Request.Method = HttpMethods.Get;
        context.Response.Body = new MemoryStream();

        using var activity = new Activity("http-request");
        activity.Start();

        // Act
        await middleware.Invoke(context);

        // Assert
        await Assert.That(activity.Status).IsEqualTo(ActivityStatusCode.Unset);
        await Assert.That(activity.TagObjects.Any(t => t.Key == "redata.error.message")).IsFalse();
    }
}
