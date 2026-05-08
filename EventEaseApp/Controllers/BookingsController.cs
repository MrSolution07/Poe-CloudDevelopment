using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEaseApp.Data;
using EventEaseApp.Models;

namespace EventEaseApp.Controllers;

[Authorize(Roles = "Admin")]
public class BookingsController : Controller
{
    private readonly EventEaseContext _context;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(EventEaseContext context, ILogger<BookingsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var bookings = await _context.Bookings
            .Include(b => b.Event)
            .Include(b => b.Venue)
            .ToListAsync();
        return View(bookings);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var booking = await _context.Bookings
            .Include(b => b.Event)
            .Include(b => b.Venue)
            .FirstOrDefaultAsync(b => b.BookingId == id);
        if (booking == null) return NotFound();

        return View(booking);
    }

    // Consolidated booking view with search and filtering (Part 2 & 3).
    // Uses the SQL-mapped vw_BookingDetail when a relational provider is in
    // use; falls back to a LINQ join (parity with the view's columns) for
    // the InMemory provider in development.
    public async Task<IActionResult> Overview(BookingSearchViewModel model)
    {
        IQueryable<BookingDetailViewModel> query;

        var providerName = _context.Database.ProviderName ?? string.Empty;
        if (providerName.Contains("InMemory", StringComparison.OrdinalIgnoreCase))
        {
            query = from b in _context.Bookings
                    join e in _context.Events on b.EventId equals e.EventId
                    join v in _context.Venues on b.VenueId equals v.VenueId
                    join et in _context.EventTypes on e.EventTypeId equals et.EventTypeId into etGroup
                    from et in etGroup.DefaultIfEmpty()
                    select new BookingDetailViewModel
                    {
                        BookingId = b.BookingId,
                        BookingDate = b.BookingDate,
                        EventId = e.EventId,
                        EventName = e.EventName,
                        EventDate = e.EventDate,
                        Description = e.Description,
                        EventImageUrl = e.ImageUrl,
                        EventTypeName = et != null ? et.Name : null,
                        VenueId = v.VenueId,
                        VenueName = v.VenueName,
                        Location = v.Location,
                        Capacity = v.Capacity,
                        ImageUrl = v.ImageUrl,
                        IsAvailable = v.IsAvailable
                    };
        }
        else
        {
            query = _context.BookingDetailView.Select(b => new BookingDetailViewModel
            {
                BookingId = b.BookingId,
                BookingDate = b.BookingDate,
                EventId = b.EventId,
                EventName = b.EventName,
                EventDate = b.EventDate,
                Description = b.EventDescription,
                EventImageUrl = b.EventImageUrl,
                EventTypeName = b.EventTypeName,
                VenueId = b.VenueId,
                VenueName = b.VenueName,
                Location = b.Location,
                Capacity = b.Capacity,
                ImageUrl = b.VenueImageUrl,
                IsAvailable = b.IsAvailable
            });
        }

        if (!string.IsNullOrWhiteSpace(model.SearchTerm))
        {
            var term = model.SearchTerm.Trim();
            if (int.TryParse(term, out int bookingId))
            {
                query = query.Where(b => b.BookingId == bookingId);
            }
            else
            {
                query = query.Where(b => b.EventName.Contains(term));
            }
        }

        if (model.EventTypeId.HasValue)
        {
            var eventTypeId = model.EventTypeId.Value;
            var eventIds = _context.Events
                .Where(e => e.EventTypeId == eventTypeId)
                .Select(e => e.EventId);
            query = query.Where(b => eventIds.Contains(b.EventId));
        }

        if (model.DateFrom.HasValue)
            query = query.Where(b => b.EventDate >= model.DateFrom.Value);

        if (model.DateTo.HasValue)
            query = query.Where(b => b.EventDate <= model.DateTo.Value);

        if (model.IsAvailable.HasValue)
            query = query.Where(b => b.IsAvailable == model.IsAvailable.Value);

        model.Results = await query.OrderByDescending(b => b.BookingDate).ToListAsync();
        model.EventTypes = await _context.EventTypes.ToListAsync();

        return View(model);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Booking booking)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns(booking.EventId, booking.VenueId);
            return View(booking);
        }

        var validationError = await ValidateBookingAsync(booking, isEdit: false);
        if (validationError != null)
        {
            TempData["ErrorMessage"] = validationError;
            await PopulateDropdowns(booking.EventId, booking.VenueId);
            return View(booking);
        }

        try
        {
            _context.Add(booking);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Booking created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex,
                "Database error while creating booking for event {EventId} venue {VenueId} on {Date}",
                booking.EventId, booking.VenueId, booking.BookingDate);
            ModelState.AddModelError(string.Empty,
                "This booking could not be saved. The venue may already be booked on the selected date or the event already has a booking.");
            TempData["ErrorMessage"] = "Could not save the booking due to a database conflict.";
            await PopulateDropdowns(booking.EventId, booking.VenueId);
            return View(booking);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating booking");
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
            TempData["ErrorMessage"] = "An error occurred while creating the booking.";
            await PopulateDropdowns(booking.EventId, booking.VenueId);
            return View(booking);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null) return NotFound();

        await PopulateDropdowns(booking.EventId, booking.VenueId);
        return View(booking);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Booking booking)
    {
        if (id != booking.BookingId) return NotFound();

        if (!ModelState.IsValid)
        {
            await PopulateDropdowns(booking.EventId, booking.VenueId);
            return View(booking);
        }

        var validationError = await ValidateBookingAsync(booking, isEdit: true);
        if (validationError != null)
        {
            TempData["ErrorMessage"] = validationError;
            await PopulateDropdowns(booking.EventId, booking.VenueId);
            return View(booking);
        }

        try
        {
            _context.Update(booking);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Booking updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Bookings.AnyAsync(b => b.BookingId == id))
                return NotFound();
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex,
                "Database error while editing booking {BookingId} for event {EventId} venue {VenueId} on {Date}",
                booking.BookingId, booking.EventId, booking.VenueId, booking.BookingDate);
            ModelState.AddModelError(string.Empty,
                "This booking could not be saved. The venue may already be booked on the selected date.");
            TempData["ErrorMessage"] = "Could not save the booking due to a database conflict.";
            await PopulateDropdowns(booking.EventId, booking.VenueId);
            return View(booking);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while editing booking {BookingId}", id);
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
            TempData["ErrorMessage"] = "An error occurred while updating the booking.";
            await PopulateDropdowns(booking.EventId, booking.VenueId);
            return View(booking);
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var booking = await _context.Bookings
            .Include(b => b.Event)
            .Include(b => b.Venue)
            .FirstOrDefaultAsync(b => b.BookingId == id);
        if (booking == null) return NotFound();

        return View(booking);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null) return NotFound();

        try
        {
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Booking deleted successfully.";
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while deleting booking {BookingId}", id);
            TempData["ErrorMessage"] = "Could not delete the booking due to a database error.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting booking {BookingId}", id);
            TempData["ErrorMessage"] = "An unexpected error occurred while deleting the booking.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<string?> ValidateBookingAsync(Booking booking, bool isEdit)
    {
        var ev = await _context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EventId == booking.EventId);
        if (ev == null)
        {
            ModelState.AddModelError(nameof(Booking.EventId),
                "The selected event could not be found. Please choose a valid event.");
            return "The selected event could not be found.";
        }

        var eventDay = ev.EventDate.Date;

        if (booking.BookingDate != default && booking.BookingDate.Date != eventDay)
        {
            ModelState.AddModelError(nameof(Booking.BookingDate),
                "Booking date must match the event date.");
            return "Booking date must match the event date.";
        }

        booking.BookingDate = eventDay;

        if (booking.BookingDate.Date < DateTime.Today)
        {
            ModelState.AddModelError(nameof(Booking.BookingDate),
                "Event date cannot be in the past. Please select a future date.");
            return "Event date cannot be in the past.";
        }

        var venue = await _context.Venues.AsNoTracking().FirstOrDefaultAsync(v => v.VenueId == booking.VenueId);
        if (venue != null && !venue.IsAvailable)
        {
            ModelState.AddModelError(nameof(Booking.VenueId),
                "The selected venue is currently unavailable. Please choose a different venue.");
            return "The selected venue is currently unavailable.";
        }

        bool eventAlreadyBooked = await _context.Bookings.AnyAsync(b =>
            b.EventId == booking.EventId &&
            (!isEdit || b.BookingId != booking.BookingId));
        if (eventAlreadyBooked)
        {
            ModelState.AddModelError(nameof(Booking.EventId),
                "This event has already been booked. Please select a different event.");
            return "This event has already been booked.";
        }

        bool doubleBooked = await _context.Bookings.AnyAsync(b =>
            b.VenueId == booking.VenueId &&
            b.BookingDate.Date == booking.BookingDate.Date &&
            (!isEdit || b.BookingId != booking.BookingId));

        if (doubleBooked)
        {
            ModelState.AddModelError(string.Empty,
                "This venue is already booked on the selected date. Please choose a different date or venue.");
            return "This venue is already booked on the selected date.";
        }

        return null;
    }

    private async Task PopulateDropdowns(int? selectedEventId = null, int? selectedVenueId = null)
    {
        ViewBag.EventId = new SelectList(
            await _context.Events.ToListAsync(), "EventId", "EventName", selectedEventId);
        ViewBag.VenueId = new SelectList(
            await _context.Venues.Where(v => v.IsAvailable).ToListAsync(), "VenueId", "VenueName", selectedVenueId);
    }
}
