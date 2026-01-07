using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;
using FastEndpoints;
using Pattern.Unions;
using ReData.DemoApp.Extensions;

namespace ReData.DemoApp.CommandMiddleware;

public sealed class TraceCommandMiddleware<TCommand, TResult> : ICommandMiddleware<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private static readonly string CommandTypeName =
        typeof(TCommand).Name.EndsWith("Command", StringComparison.Ordinal)
            ? typeof(TCommand).Name[..^7]
            : typeof(TCommand).Name;


    /// <inheritdoc />
    public async Task<TResult> ExecuteAsync(TCommand command, CommandDelegate<TResult> next, CancellationToken ct)
    {
        using var span = Tracing.ReData.StartActivity(CommandTypeName);

        try
        {
            var response = await next().ConfigureAwait(false); // There is no argument for cancellation token

            if (response is IError)
            {
                span?.SetTag("command.success", false);
                span?.SetStatus(ActivityStatusCode.Error);
            }
            else
            {
                span?.SetTag("command.success", true);
            }

            return response;
        }
        catch (Exception ex)
        {
            span?.SetTag("command.success", false);
            span?.AddException(ex);
            span?.SetStatus(ActivityStatusCode.Error);
            throw;
        }
    }
}