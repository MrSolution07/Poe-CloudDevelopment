using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEaseApp.Data;
using EventEaseApp.Models;
using EventEaseApp.Services;

namespace EventEaseApp.Controllers;

[Authorize]
public class VenuesController : Controller
{
    private readonly EventEaseContext _context;
    private readonly IBlobStorageService _blobService;
    private readonly string _containerName;

    public VenuesController(EventEaseContext context, IBlobStorageService blobService, IConfiguration config)
    {
        _context = context;
        _blobService = blobService;
        _containerName = config["AzureBlobStorage:VenueContainerName"] ?? "venue-images";
    }

    public async Task<IActionResult> Index()
    {
        var venues = await _context.Venues.ToListAsync();
        return View(venues);
    }

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
    public async Task<IActionResult> Create(Venue venue, IFormFile? imageFile)
    {
        if (!ModelState.IsValid)
            return View(venue);

        try
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                if (_blobService.IsConfigured)
                    venue.ImageUrl = await _blobService.UploadImageAsync(imageFile, _containerName);
                else
                    TempData["ErrorMessage"] = "Azure Blob Storage is not configured. Image was not uploaded — use an image URL instead.";
            }

            _context.Add(venue);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] ??= "Venue created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "An error occurred while creating the venue.";
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
    public async Task<IActionResult> Edit(int id, Venue venue, IFormFile? imageFile)
    {
        if (id != venue.VenueId) return NotFound();

        if (!ModelState.IsValid)
            return View(venue);

        try
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                if (_blobService.IsConfigured)
                {
                    if (!string.IsNullOrEmpty(venue.ImageUrl))
                        await _blobService.DeleteImageAsync(venue.ImageUrl, _containerName);

                    venue.ImageUrl = await _blobService.UploadImageAsync(imageFile, _containerName);
                }
                else
                {
                    TempData["ErrorMessage"] = "Azure Blob Storage is not configured. Image was not uploaded — use an image URL instead.";
                }
            }

            _context.Update(venue);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] ??= "Venue updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Venues.AnyAsync(v => v.VenueId == id))
                return NotFound();
            throw;
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "An error occurred while updating the venue.";
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
        catch (Exception)
        {
            TempData["ErrorMessage"] = "An error occurred while deleting the venue.";
        }

        return RedirectToAction(nameof(Index));
    }
}
