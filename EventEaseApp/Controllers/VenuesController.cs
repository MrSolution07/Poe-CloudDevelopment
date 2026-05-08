using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEaseApp.Data;
using EventEaseApp.Models;
using EventEaseApp.Services;

namespace EventEaseApp.Controllers;

[Authorize(Roles = "Admin")]
public class VenuesController : Controller
{
    private readonly EventEaseContext _context;
    private readonly IBlobStorageService _blobService;
    private readonly IImageProcessingService _imageProcessor;
    private readonly ILogger<VenuesController> _logger;
    private readonly string _containerName;

    public VenuesController(
        EventEaseContext context,
        IBlobStorageService blobService,
        IImageProcessingService imageProcessor,
        IConfiguration config,
        ILogger<VenuesController> logger)
    {
        _context = context;
        _blobService = blobService;
        _imageProcessor = imageProcessor;
        _logger = logger;
        _containerName = config["AzureBlobStorage:VenueContainerName"] ?? "venue-images";
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var venues = await _context.Venues.ToListAsync();
        return View(venues);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var venue = await _context.Venues.FirstOrDefaultAsync(v => v.VenueId == id);
        if (venue == null) return NotFound();

        return View(venue);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(20_971_520)]
    public async Task<IActionResult> Create(Venue venue, IFormFile? imageFile)
    {
        if (!ModelState.IsValid)
            return View(venue);

        try
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                if (!_blobService.IsConfigured)
                {
                    ModelState.AddModelError("imageFile",
                        "Image upload is unavailable because Azure Blob Storage is not configured. Provide an image URL instead.");
                    TempData["ErrorMessage"] = "Image upload unavailable; provide a URL instead.";
                    return View(venue);
                }

                using var processed = await _imageProcessor.ProcessAsync(imageFile);
                venue.ImageUrl = await _blobService.UploadProcessedImageAsync(
                    processed.Content, processed.ContentType, processed.Extension, _containerName);
            }

            _context.Add(venue);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Venue created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidImageUploadException ex)
        {
            ModelState.AddModelError("imageFile", ex.Message);
            TempData["ErrorMessage"] = ex.Message;
            return View(venue);
        }
        catch (Azure.RequestFailedException ex)
        {
            _logger.LogError(ex, "Azure Blob upload failed while creating venue {VenueName}", venue.VenueName);
            ModelState.AddModelError(string.Empty,
                "We could not upload the image to cloud storage. Please try again.");
            TempData["ErrorMessage"] = "We could not upload the image to cloud storage. Please try again.";
            return View(venue);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while creating venue {VenueName}", venue.VenueName);
            ModelState.AddModelError(string.Empty,
                "We could not save the venue due to a database error. Please review your input and try again.");
            TempData["ErrorMessage"] = "Could not save the venue. Please try again.";
            return View(venue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating venue {VenueName}", venue.VenueName);
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
            TempData["ErrorMessage"] = "An unexpected error occurred while creating the venue.";
            return View(venue);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var venue = await _context.Venues.FindAsync(id);
        if (venue == null) return NotFound();

        return View(venue);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(20_971_520)]
    public async Task<IActionResult> Edit(int id, Venue venue, IFormFile? imageFile)
    {
        if (id != venue.VenueId) return NotFound();

        if (!ModelState.IsValid)
            return View(venue);

        try
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                if (!_blobService.IsConfigured)
                {
                    ModelState.AddModelError("imageFile",
                        "Image upload is unavailable because Azure Blob Storage is not configured. Provide an image URL instead.");
                    TempData["ErrorMessage"] = "Image upload unavailable; provide a URL instead.";
                    return View(venue);
                }

                using var processed = await _imageProcessor.ProcessAsync(imageFile);
                if (!string.IsNullOrEmpty(venue.ImageUrl))
                    await _blobService.DeleteImageAsync(venue.ImageUrl, _containerName);

                venue.ImageUrl = await _blobService.UploadProcessedImageAsync(
                    processed.Content, processed.ContentType, processed.Extension, _containerName);
            }

            _context.Update(venue);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Venue updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidImageUploadException ex)
        {
            ModelState.AddModelError("imageFile", ex.Message);
            TempData["ErrorMessage"] = ex.Message;
            return View(venue);
        }
        catch (Azure.RequestFailedException ex)
        {
            _logger.LogError(ex, "Azure Blob upload failed while editing venue {VenueId}", id);
            ModelState.AddModelError(string.Empty,
                "We could not upload the image to cloud storage. Please try again.");
            TempData["ErrorMessage"] = "We could not upload the image to cloud storage. Please try again.";
            return View(venue);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Venues.AnyAsync(v => v.VenueId == id))
                return NotFound();
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while editing venue {VenueId}", id);
            ModelState.AddModelError(string.Empty,
                "We could not save the venue due to a database error. Please review your input and try again.");
            TempData["ErrorMessage"] = "Could not save the venue. Please try again.";
            return View(venue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while editing venue {VenueId}", id);
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
            TempData["ErrorMessage"] = "An unexpected error occurred while updating the venue.";
            return View(venue);
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var venue = await _context.Venues.FirstOrDefaultAsync(v => v.VenueId == id);
        if (venue == null) return NotFound();

        return View(venue);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var venue = await _context.Venues.FindAsync(id);
        if (venue == null) return NotFound();

        bool hasBookings = await _context.Bookings.AnyAsync(b => b.VenueId == id);
        if (hasBookings)
        {
            TempData["ErrorMessage"] = "Cannot delete this venue because it has active bookings.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            if (!string.IsNullOrEmpty(venue.ImageUrl))
                await _blobService.DeleteImageAsync(venue.ImageUrl, _containerName);

            _context.Venues.Remove(venue);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Venue deleted successfully.";
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while deleting venue {VenueId}", id);
            TempData["ErrorMessage"] = "Could not delete the venue due to a database error.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting venue {VenueId}", id);
            TempData["ErrorMessage"] = "An unexpected error occurred while deleting the venue.";
        }

        return RedirectToAction(nameof(Index));
    }
}
