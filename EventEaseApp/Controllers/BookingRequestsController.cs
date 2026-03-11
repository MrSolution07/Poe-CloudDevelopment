using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEaseApp.Data;
using EventEaseApp.Models;

namespace EventEaseApp.Controllers;

public class BookingRequestsController : Controller
{
    private readonly EventEaseContext _context;

    public BookingRequestsController(EventEaseContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index(string? status)
    {
        var query = _context.BookingRequests
            .Include(br => br.PreferredVenue)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(br => br.Status == status);

        var requests = await query.OrderByDescending(br => br.CreatedAt).ToListAsync();
        ViewBag.CurrentStatus = status;
        return View(requests);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Create()
    {
        ViewBag.VenueId = new SelectList(
            await _context.Venues.Where(v => v.IsAvailable).ToListAsync(),
            "VenueId", "VenueName");
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingRequest request)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.VenueId = new SelectList(
                await _context.Venues.Where(v => v.IsAvailable).ToListAsync(),
                "VenueId", "VenueName", request.PreferredVenueId);
            return View(request);
        }

        try
        {
            request.Status = "Pending";
            request.CreatedAt = DateTime.UtcNow;
            _context.Add(request);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Your booking request has been submitted. Our team will contact you shortly.";
            return RedirectToAction("Confirmation");
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "An error occurred while submitting your request. Please try again.";
            ViewBag.VenueId = new SelectList(
                await _context.Venues.Where(v => v.IsAvailable).ToListAsync(),
                "VenueId", "VenueName", request.PreferredVenueId);
            return View(request);
        }
    }

    [AllowAnonymous]
    public IActionResult Confirmation()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkProcessed(int id)
    {
        var request = await _context.BookingRequests.FindAsync(id);
        if (request == null) return NotFound();

        request.Status = "Processed";
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Request #{id} marked as processed.";
        return RedirectToAction(nameof(Index));
    }
}
