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
    private readonly string _containerName;

    public VenuesController(EventEaseContext context, IBlobStorageService blobService, IConfiguration config)
    {
        _context = context;
        _blobService = blobService;
        _containerName = config["AzureBlobStorage:VenueContainerName"] ?? "venue-images";
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var venues = await _context.Venues.ToListAsync();
        // #region agent log H-C/H-D
        Console.Error.WriteLine("[DBG-f875ef][H-C] Index: count=" + venues.Count + " isAdmin=" + User.IsInRole("Admin") + " ids=" + string.Join(",", venues.Select(v => v.VenueId + ":" + v.VenueName + ":hasImg=" + !string.IsNullOrEmpty(v.ImageUrl))));
        // #endregion
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
    public async Task<IActionResult> Create(Venue venue, IFormFile? imageFile)
    {
        if (!ModelState.IsValid)
            return View(venue);

        try
        {
            // #region agent log H-A/H-B
            Console.Error.WriteLine("[DBG-f875ef][H-A] Create: imageFile=" + (imageFile == null ? "null" : imageFile.FileName + ":" + imageFile.Length) + " blobConfigured=" + _blobService.IsConfigured);
            // #endregion
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
        catch (Exception ex)
        {
            // #region agent log H-A/H-B
            Console.Error.WriteLine("[DBG-f875ef][H-A] Create caught: type=" + ex.GetType().Name + " msg=" + ex.Message + " inner=" + ex.InnerException?.Message);
            TempData["ErrorMessage"] = $"[DBG] {ex.GetType().Name}: {ex.Message.Substring(0, Math.Min(300, ex.Message.Length))}";
            // #endregion
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
