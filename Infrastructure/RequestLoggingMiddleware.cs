using System.Diagnostics;

namespace FlightOps.Infrastructure;

public sealed class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    private const string CorrelationHeader = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context)
    {
        string correlationId = context.Request.Headers[CorrelationHeader].FirstOrDefault()
            ?? Activity.Current?.Id
            ?? context.TraceIdentifier;

        context.Response.Headers[CorrelationHeader] = correlationId;

        Stopwatch stopwatch = Stopwatch.StartNew();
        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();

            if (!IsStaticAssetPath(context.Request.Path))
            {
                logger.LogInformation(
                    "{Method} {Path} responded {StatusCode} in {ElapsedMs}ms ({CorrelationId})",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    correlationId);
            }
        }
    }

    private static bool IsStaticAssetPath(PathString path)
    {
        if (!path.HasValue)
            return false;

        string value = path.Value!;

        if (value.StartsWith("/css/", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/js/", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/lib/", StringComparison.OrdinalIgnoreCase))
            return true;

        if (value.Equals("/FlightOps.styles.css", StringComparison.OrdinalIgnoreCase))
            return true;

        return value.Equals("/favicon.ico", StringComparison.OrdinalIgnoreCase)
            || value.Equals("/favicon.png", StringComparison.OrdinalIgnoreCase)
            || value.Equals("/apple-touch-icon.png", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/favicon-", StringComparison.OrdinalIgnoreCase);
    }
}
