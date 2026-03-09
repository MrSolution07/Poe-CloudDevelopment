using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEaseApp.Data;
using EventEaseApp.Models;
using EventEaseApp.Services;

namespace EventEaseApp.Controllers;

public class EventsController : Controller
{
    private readonly EventEaseContext _context;
    private readonly IBlobStorageService _blobService;
    private readonly string _containerName;

    public EventsController(EventEaseContext context, IBlobStorageService blobService, IConfiguration config)
    {
        _context = context;
        _blobService = blobService;
        _containerName = config["AzureBlobStorage:EventContainerName"] ?? "event-images";
    }

    public async Task<IActionResult> Index()
    {
        var events = await _context.Events
            .Include(e => e.Venue)
            .Include(e => e.EventType)
            .ToListAsync();
        return View(events);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var ev = await _context.Events
            .Include(e => e.Venue)
            .Include(e => e.EventType)
            .FirstOrDefaultAsync(e => e.EventId == id);
        if (ev == null) return NotFound();

        return View(ev);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Event ev, IFormFile? imageFile)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns(ev.VenueId, ev.EventTypeId);
            return View(ev);
        }

        try
        {
            if (imageFile != null && imageFile.Length > 0)
                ev.ImageUrl = await _blobService.UploadImageAsync(imageFile, _containerName);

            _context.Add(ev);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Event created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "An error occurred while creating the event.";
            await PopulateDropdowns(ev.VenueId, ev.EventTypeId);
            return View(ev);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var ev = await _context.Events.FindAsync(id);
        if (ev == null) return NotFound();

        await PopulateDropdowns(ev.VenueId, ev.EventTypeId);
        return View(ev);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Event ev, IFormFile? imageFile)
    {
        if (id != ev.EventId) return NotFound();

        if (!ModelState.IsValid)
        {
            await PopulateDropdowns(ev.VenueId, ev.EventTypeId);
            return View(ev);
        }

        try
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(ev.ImageUrl))
                    await _blobService.DeleteImageAsync(ev.ImageUrl, _containerName);

                ev.ImageUrl = await _blobService.UploadImageAsync(imageFile, _containerName);
            }

            _context.Update(ev);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Event updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Events.AnyAsync(e => e.EventId == id))
                return NotFound();
            throw;
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "An error occurred while updating the event.";
            await PopulateDropdowns(ev.VenueId, ev.EventTypeId);
            return View(ev);
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var ev = await _context.Events
            .Include(e => e.Venue)
            .Include(e => e.EventType)
            .FirstOrDefaultAsync(e => e.EventId == id);
        if (ev == null) return NotFound();

        return View(ev);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev == null) return NotFound();

        bool hasBookings = await _context.Bookings.AnyAsync(b => b.EventId == id);
        if (hasBookings)
        {
            TempData["ErrorMessage"] = "Cannot delete this event because it has active bookings.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            if (!string.IsNullOrEmpty(ev.ImageUrl))
                await _blobService.DeleteImageAsync(ev.ImageUrl, _containerName);

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Event deleted successfully.";
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "An error occurred while deleting the event.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdowns(int? selectedVenueId = null, int? selectedEventTypeId = null)
    {
        ViewBag.VenueId = new SelectList(
            await _context.Venues.ToListAsync(), "VenueId", "VenueName", selectedVenueId);
        ViewBag.EventTypeId = new SelectList(
            await _context.EventTypes.ToListAsync(), "EventTypeId", "Name", selectedEventTypeId);
    }
}
