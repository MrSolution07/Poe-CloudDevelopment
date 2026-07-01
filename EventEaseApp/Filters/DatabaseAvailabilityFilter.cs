using EventEaseApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EventEaseApp.Filters;

public sealed class DatabaseAvailabilityFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var dbState = context.HttpContext.RequestServices.GetRequiredService<DatabaseAvailabilityState>();
        if (dbState.IsAvailable || IsDatabaseUnavailablePage(context))
        {
            await next();
            return;
        }

        context.Result = new RedirectToActionResult("DatabaseUnavailable", "Home", null);
    }

    private static bool IsDatabaseUnavailablePage(ActionExecutingContext context)
    {
        var controller = context.RouteData.Values["controller"]?.ToString();
        var action = context.RouteData.Values["action"]?.ToString();
        return string.Equals(controller, "Home", StringComparison.OrdinalIgnoreCase)
            && string.Equals(action, "DatabaseUnavailable", StringComparison.OrdinalIgnoreCase);
    }
}
