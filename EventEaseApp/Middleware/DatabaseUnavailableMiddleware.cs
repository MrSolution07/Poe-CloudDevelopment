using EventEaseApp.Data;

namespace EventEaseApp.Middleware;

public class DatabaseUnavailableMiddleware
{
    private static readonly PathString UnavailablePath = new("/Home/DatabaseUnavailable");
    private readonly RequestDelegate _next;

    public DatabaseUnavailableMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, DatabaseAvailabilityState dbState)
    {
        if (dbState.IsAvailable || IsExempt(context.Request.Path))
        {
            await _next(context);
            return;
        }

        if (context.Request.Path.StartsWithSegments(UnavailablePath))
        {
            await _next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        context.Response.Redirect(UnavailablePath.Value!);
        return;
    }

    private static bool IsExempt(PathString path)
    {
        var value = path.Value ?? string.Empty;
        return value.StartsWith("/Home/Error", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/css/", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/js/", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/lib/", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/images/", StringComparison.OrdinalIgnoreCase)
            || value.Contains("/_content/", StringComparison.OrdinalIgnoreCase)
            || value.Equals("/favicon.ico", StringComparison.OrdinalIgnoreCase)
            || value.EndsWith(".css", StringComparison.OrdinalIgnoreCase)
            || value.EndsWith(".js", StringComparison.OrdinalIgnoreCase);
    }
}
