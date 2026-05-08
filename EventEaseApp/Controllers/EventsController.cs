using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEaseApp.Data;
using EventEaseApp.Models;
using EventEaseApp.Services;

namespace EventEaseApp.Controllers;

[Authorize(Roles = "Admin")]
public class EventsController : Controller
{
    private readonly EventEaseContext _context;
    private readonly IBlobStorageService _blobService;
    private readonly IImageProcessingService _imageProcessor;
    private readonly ILogger<EventsController> _logger;
    private readonly string _containerName;

    public EventsController(
        EventEaseContext context,
        IBlobStorageService blobService,
        IImageProcessingService imageProcessor,
        IConfiguration config,
        ILogger<EventsController> logger)
    {
        _context = context;
        _blobService = blobService;
        _imageProcessor = imageProcessor;
        _logger = logger;
        _containerName = config["AzureBlobStorage:EventContainerName"] ?? "event-images";
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var events = await _context.Events
            .Include(e => e.Venue)
            .Include(e => e.EventType)
            .ToListAsync();
        return View(events);
    }

    [AllowAnonymous]
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
    [RequestSizeLimit(20_971_520)]
    public async Task<IActionResult> Create(Event ev, IFormFile? imageFile)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns(ev.VenueId, ev.EventTypeId);
            return View(ev);
        }

        if (ev.EventDate.Date < DateTime.Today)
        {
            ModelState.AddModelError(nameof(Event.EventDate),
                "Event date cannot be in the past. Please select today or a future date.");
            TempData["ErrorMessage"] = "Event date cannot be in the past.";
            await PopulateDropdowns(ev.VenueId, ev.EventTypeId);
            return View(ev);
        }

        try
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                if (!_blobService.IsConfigured)
                {
                    ModelState.AddModelError("imageFile",
                        "Image upload is disabled in this environment because Azure Blob Storage has not been configured. Remove the file to save without an image, or paste a public image URL into the Image URL field.");
                    TempData["ErrorMessage"] = "Image upload is disabled — Azure Blob Storage is not configured.";
                    await PopulateDropdowns(ev.VenueId, ev.EventTypeId);
                    return View(ev);
                }

                using var processed = await _imageProcessor.ProcessAsync(imageFile);
                ev.ImageUrl = await _blobService.UploadProcessedImageAsync(
                    processed.Content, processed.ContentType, processed.Extension, _containerName);
            }

            _context.Add(ev);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Event created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidImageUploadException ex)
        {
            ModelState.AddModelError("imageFile", ex.Message);
            TempData["ErrorMessage"] = ex.Message;
            await PopulateDropdowns(ev.VenueId, ev.EventTypeId);
            return View(ev);
        }
        catch (Azure.RequestFailedException ex)
        {
            _logger.LogError(ex, "Azure Blob upload failed while creating event {EventName}", ev.EventName);
            ModelState.AddModelError("imageFile",
                "Azure Blob Storage rejected the upload. Verify the storage account connection string and that the storage account is reachable.");
            TempData["ErrorMessage"] = "Azure Blob Storage rejected the upload — check the connection string.";
            await PopulateDropdowns(ev.VenueId, ev.EventTypeId);
            return View(ev);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while creating event {EventName}", ev.EventName);
            ModelState.AddModelError(string.Empty,
                "We could not save the event due to a database error. Please review your input and try again.");
            TempData["ErrorMessage"] = "Could not save the event. Please try again.";
            await PopulateDropdowns(ev.VenueId, ev.EventTypeId);
            return View(ev);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating event {EventName}", ev.EventName);
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
            TempData["ErrorMessage"] = "An unexpected error occurred while creating the event.";
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
    [RequestSizeLimit(20_971_520)]
    public async Task<IActionResult> Edit(int id, Event ev, IFormFile? imageFile)
    {
        if (id != ev.EventId) return NotFound();

        if (!ModelState.IsValid)
        {
            await PopulateDropdowns(ev.VenueId, ev.EventTypeId);
            return View(ev);
        }

        if (ev.EventDate.Date < DateTime.Today)
        {
            ModelState.AddModelError(nameof(Event.EventDate),
                "Event date cannot be in the past. Please select today or a future date.");
            TempData["ErrorMessage"] = "Event date cannot be in the past.";
            await PopulateDropdowns(ev.VenueId, ev.EventTypeId);
            return View(ev);
        }

        try
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                if (!_blobService.IsConfigured)
                {
                    ModelState.AddModelError("imageFile",
                        "Image upload is disabled in this environment because Azure Blob Storage has not been configured. Remove the file to save without changing the image, or paste a public image URL into the Image URL field.");
                    TempData["ErrorMessage"] = "Image upload is disabled — Azure Blob Storage is not configured.";
                    await PopulateDropdowns(ev.VenueId, ev.EventTypeId);
                    return View(ev);
                }

                using var processed = await _imageProcessor.ProcessAsync(imageFile);
                if (!string.IsNullOrEmpty(ev.ImageUrl))
                    await _blobService.DeleteImageAsync(ev.ImageUrl, _containerName);

                ev.ImageUrl = await _blobService.UploadProcessedImageAsync(
                    processed.Content, processed.ContentType, processed.Extension, _containerName);
            }

            _context.Update(ev);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Event updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidImageUploadException ex)
        {
            ModelState.AddModelError("imageFile", ex.Message);
            TempData["ErrorMessage"] = ex.Message;
            await PopulateDropdowns(ev.VenueId, ev.EventTypeId);
            return View(ev);
        }
        catch (Azure.RequestFailedException ex)
        {
            _logger.LogError(ex, "Azure Blob upload failed while editing event {EventId}", id);
            ModelState.AddModelError("imageFile",
                "Azure Blob Storage rejected the upload. Verify the storage account connection string and that the storage account is reachable.");
            TempData["ErrorMessage"] = "Azure Blob Storage rejected the upload — check the connection string.";
            await PopulateDropdowns(ev.VenueId, ev.EventTypeId);
            return View(ev);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Events.AnyAsync(e => e.EventId == id))
                return NotFound();
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while editing event {EventId}", id);
            ModelState.AddModelError(string.Empty,
                "We could not save the event due to a database error. Please review your input and try again.");
            TempData["ErrorMessage"] = "Could not save the event. Please try again.";
            await PopulateDropdowns(ev.VenueId, ev.EventTypeId);
            return View(ev);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while editing event {EventId}", id);
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
            TempData["ErrorMessage"] = "An unexpected error occurred while updating the event.";
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
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while deleting event {EventId}", id);
            TempData["ErrorMessage"] = "Could not delete the event due to a database error.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting event {EventId}", id);
            TempData["ErrorMessage"] = "An unexpected error occurred while deleting the event.";
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
