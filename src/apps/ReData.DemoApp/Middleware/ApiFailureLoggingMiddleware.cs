using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace ReData.DemoApp.Middleware;

/// <summary>
/// Middleware для логирования ошибок API.
/// </summary>
public sealed class ApiFailureLoggingMiddleware(RequestDelegate next)
{
    private const int MaxBodyBytes = 64 * 1024;

    /// <inheritdoc />
    public async Task Invoke(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        var originalBody = context.Response.Body;
        await using var captureStream = new ResponseCaptureStream(originalBody, MaxBodyBytes);
        context.Response.Body = captureStream;

        Exception? requestException = null;
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            requestException = ex;
            throw;
        }
        finally
        {
            context.Response.Body = originalBody;
            await captureStream.FlushAsync();

            var activity = Activity.Current;
            if (activity is not null)
            {
                if (requestException is not null)
                {
                    var statusCode = context.Response.StatusCode >= StatusCodes.Status400BadRequest
                        ? context.Response.StatusCode
                        : StatusCodes.Status500InternalServerError;

                    ApplyErrorTags(
                        activity: activity,
                        statusCode: statusCode,
                        message: requestException.Message,
                        problemType: null,
                        problemTitle: null,
                        problemDetail: null,
                        errors: null);

                    activity.SetTag("exception.type", requestException.GetType().FullName);
                    activity.SetTag("exception.message", requestException.Message);
                }
                else if (context.Response.StatusCode >= StatusCodes.Status400BadRequest)
                {
                    var payload = ErrorPayload.TryParse(captureStream.GetCapturedText());
                    var message = payload?.Message
                        ?? payload?.ProblemDetail
                        ?? payload?.ProblemTitle
                        ?? $"HTTP {context.Response.StatusCode}";

                    ApplyErrorTags(
                        activity: activity,
                        statusCode: context.Response.StatusCode,
                        message: message,
                        problemType: payload?.ProblemType,
                        problemTitle: payload?.ProblemTitle,
                        problemDetail: payload?.ProblemDetail,
                        errors: payload?.Errors);
                }
            }
        }
    }

    private static void ApplyErrorTags(
        Activity activity,
        int statusCode,
        string? message,
        string? problemType,
        string? problemTitle,
        string? problemDetail,
        IReadOnlyDictionary<string, string>? errors)
    {
        activity.SetTag("error", true);
        SetIfNotEmpty(activity, "redata.error.message", message);
        SetIfNotEmpty(activity, "redata.error.problem_type", problemType);
        SetIfNotEmpty(activity, "redata.error.problem_title", problemTitle);
        SetIfNotEmpty(activity, "redata.error.problem_detail", problemDetail);

        if (errors is not null)
        {
            foreach (var error in errors)
            {
                SetIfNotEmpty(activity, $"redata.error.{error.Key}", error.Value);
            }
        }

        activity.SetStatus(ActivityStatusCode.Error, message ?? $"HTTP {statusCode}");
    }

    private static void SetIfNotEmpty(Activity activity, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            activity.SetTag(key, value);
        }
    }

    private sealed class ErrorPayload
    {
        public string? Message { get; init; }
        public string? ProblemType { get; init; }
        public string? ProblemTitle { get; init; }
        public string? ProblemDetail { get; init; }
        public Dictionary<string, string>? Errors { get; init; }

        public static ErrorPayload? TryParse(string? body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return null;
            }

            try
            {
                using var document = JsonDocument.Parse(body);
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                {
                    return null;
                }

                var errors = TryReadErrors(document.RootElement);
                return new ErrorPayload
                {
                    Message = TryGetString(document.RootElement, "message"),
                    ProblemType = TryGetString(document.RootElement, "type"),
                    ProblemTitle = TryGetString(document.RootElement, "title"),
                    ProblemDetail = TryGetString(document.RootElement, "detail"),
                    Errors = errors,
                };
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static Dictionary<string, string>? TryReadErrors(JsonElement root)
        {
            if (!root.TryGetProperty("errors", out var errorsNode) || errorsNode.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in errorsNode.EnumerateObject())
            {
                var value = item.Value.ValueKind switch
                {
                    JsonValueKind.Array => string.Join("; ", item.Value.EnumerateArray().Select(GetCompactValue)),
                    JsonValueKind.String => item.Value.GetString() ?? string.Empty,
                    _ => item.Value.GetRawText()
                };

                if (!string.IsNullOrWhiteSpace(value))
                {
                    result[item.Name] = Shorten(value, 4096);
                }
            }

            return result.Count == 0 ? null : result;
        }

        private static string GetCompactValue(JsonElement value)
        {
            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString() ?? string.Empty,
                _ => value.GetRawText()
            };
        }

        private static string? TryGetString(JsonElement root, string name)
        {
            if (!root.TryGetProperty(name, out var value))
            {
                return null;
            }

            return value.ValueKind == JsonValueKind.String ? value.GetString() : null;
        }
    }

    /// <summary>
    /// Обрезка строки до указанного количества символов.
    /// </summary>
    private static string Shorten(string value, int maxChars)
    {
        if (value.Length <= maxChars)
        {
            return value;
        }

        return $"{value[..maxChars]}...[truncated]";
    }

    /// <summary>
    /// Логгирование тела ответа для диагностики.
    /// </summary>
    private sealed class ResponseCaptureStream(Stream inner, int maxBytes) : Stream
    {
        private readonly MemoryStream _capture = new();
        private readonly Stream _inner = inner;
        private readonly int _maxBytes = maxBytes;
        private bool _isTruncated;

        public string? GetCapturedText()
        {
            if (_capture.Length == 0)
            {
                return null;
            }

            var text = Encoding.UTF8.GetString(_capture.ToArray());
            return _isTruncated ? $"{text}...[truncated]" : text;
        }

        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => _inner.CanSeek;
        public override bool CanWrite => _inner.CanWrite;
        public override long Length => _inner.Length;
        public override long Position
        {
            get => _inner.Position;
            set => _inner.Position = value;
        }

        public override void Flush() => _inner.Flush();
        public override Task FlushAsync(CancellationToken cancellationToken) => _inner.FlushAsync(cancellationToken);
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
        public override void SetLength(long value) => _inner.SetLength(value);
        public override async ValueTask DisposeAsync()
        {
            await _capture.DisposeAsync();
            await base.DisposeAsync();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _inner.Write(buffer, offset, count);
            Capture(buffer.AsSpan(offset, count));
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await _inner.WriteAsync(buffer.AsMemory(offset, count), cancellationToken);
            Capture(buffer.AsSpan(offset, count));
        }

        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await _inner.WriteAsync(buffer, cancellationToken);
            Capture(buffer.Span);
        }

        private void Capture(ReadOnlySpan<byte> bytes)
        {
            if (_capture.Length >= _maxBytes)
            {
                _isTruncated = true;
                return;
            }

            var allowed = (int)Math.Min(_maxBytes - _capture.Length, bytes.Length);
            if (allowed < bytes.Length)
            {
                _isTruncated = true;
            }

            if (allowed > 0)
            {
                _capture.Write(bytes[..allowed]);
            }
        }
    }
}
