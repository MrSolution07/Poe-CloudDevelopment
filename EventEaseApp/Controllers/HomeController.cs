using System.Diagnostics;
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
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
