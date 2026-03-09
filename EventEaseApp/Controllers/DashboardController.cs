using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEaseApp.Data;
using EventEaseApp.Models;

namespace EventEaseApp.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly EventEaseContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(EventEaseContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var today = DateTime.Today;

        var model = new DashboardViewModel
        {
            UserName = user?.FullName ?? User.Identity?.Name ?? "Specialist",
            TotalVenues = await _context.Venues.CountAsync(),
            TotalEvents = await _context.Events.CountAsync(),
            TotalBookings = await _context.Bookings.CountAsync(),
            AvailableVenues = await _context.Venues.CountAsync(v => v.IsAvailable),
            UpcomingEvents = await _context.Events.CountAsync(e => e.EventDate >= today),
            RecentBookings = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .OrderByDescending(b => b.BookingDate)
                .Take(5)
                .ToListAsync(),
            UpcomingEventsList = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.EventType)
                .Where(e => e.EventDate >= today)
                .OrderBy(e => e.EventDate)
                .Take(5)
                .ToListAsync()
        };

        return View(model);
    }
}
