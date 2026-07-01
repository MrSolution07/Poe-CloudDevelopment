using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEaseApp.Data;
using EventEaseApp.Models;

namespace EventEaseApp.Controllers;

public class HomeController : Controller
{
    private readonly EventEaseContext _context;

    public HomeController(EventEaseContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        var today = DateTime.Today;

        ViewBag.FeaturedVenues = await _context.Venues
            .Where(v => v.IsAvailable)
            .Take(6)
            .ToListAsync();

        ViewBag.UpcomingEvents = await _context.Events
            .Include(e => e.Venue)
            .Include(e => e.EventType)
            .Where(e => e.EventDate >= today)
            .OrderBy(e => e.EventDate)
            .Take(6)
            .ToListAsync();

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult DatabaseUnavailable([FromServices] DatabaseAvailabilityState databaseAvailability)
    {
        Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        return View(databaseAvailability);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [Route("Home/Error")]
    [Route("Home/Error/{code:int}")]
    public IActionResult Error(int? code = null)
    {
        var statusCodeFromQuery = code
            ?? (int.TryParse(Request.Query["code"], out var queryCode) ? queryCode : (int?)null);

        var reExecuteFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
        var originalPath = reExecuteFeature?.OriginalPath;

        var statusCode = statusCodeFromQuery ?? HttpContext.Response.StatusCode;

        var (title, message) = ResolveCopy(statusCode);

        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            StatusCode = statusCode,
            Title = title,
            Message = message,
            OriginalPath = originalPath
        });
    }

    private static (string Title, string Message) ResolveCopy(int statusCode) => statusCode switch
    {
        400 => ("Bad request", "The server could not understand your request. Please check the address and try again."),
        401 => ("Sign in required", "You need to sign in to view this page."),
        403 => ("Access denied", "You do not have permission to view this page."),
        404 => ("Page not found", "The page you are looking for has been moved, removed, or never existed."),
        408 => ("Request timed out", "Your request took too long. Please try again."),
        500 => ("Something went wrong", "An unexpected error occurred. Our team has been notified. Please try again shortly."),
        502 or 503 or 504 => ("Service unavailable", "The service is temporarily unavailable. Please try again in a few minutes."),
        _ => ("Something went wrong", "An unexpected error occurred. Please try again.")
    };
}
