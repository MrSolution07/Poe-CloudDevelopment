using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEaseApp.Data;
using EventEaseApp.Models;

namespace EventEaseApp.Controllers;

[Authorize]
public class BookingsController : Controller
{
    private readonly EventEaseContext _context;

    public BookingsController(EventEaseContext context)
    {
        _context = context;
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

    // Consolidated booking view with search and filtering (Part 2 & 3)
    public async Task<IActionResult> Overview(BookingSearchViewModel model)
    {
        var query = from b in _context.Bookings
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
                        EventTypeName = et != null ? et.Name : null,
                        VenueId = v.VenueId,
                        VenueName = v.VenueName,
                        Location = v.Location,
                        Capacity = v.Capacity,
                        ImageUrl = v.ImageUrl,
                        IsAvailable = v.IsAvailable
                    };

        // Search by BookingID or Event Name
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

        // Filter by EventType
        if (model.EventTypeId.HasValue)
        {
            var eventTypeId = model.EventTypeId.Value;
            var eventIds = _context.Events
                .Where(e => e.EventTypeId == eventTypeId)
                .Select(e => e.EventId);
            query = query.Where(b => eventIds.Contains(b.EventId));
        }

        // Filter by date range
        if (model.DateFrom.HasValue)
            query = query.Where(b => b.BookingDate >= model.DateFrom.Value);

        if (model.DateTo.HasValue)
            query = query.Where(b => b.BookingDate <= model.DateTo.Value);

        // Filter by venue availability
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

        // Prevent double booking: same venue on same date
        bool doubleBooked = await _context.Bookings.AnyAsync(b =>
            b.VenueId == booking.VenueId &&
            b.BookingDate.Date == booking.BookingDate.Date);

        if (doubleBooked)
        {
            TempData["ErrorMessage"] = "This venue is already booked on the selected date. Please choose a different date or venue.";
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
        catch (Exception)
        {
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

        // Prevent double booking on edit (exclude current booking)
        bool doubleBooked = await _context.Bookings.AnyAsync(b =>
            b.VenueId == booking.VenueId &&
            b.BookingDate.Date == booking.BookingDate.Date &&
            b.BookingId != booking.BookingId);

        if (doubleBooked)
        {
            TempData["ErrorMessage"] = "This venue is already booked on the selected date. Please choose a different date or venue.";
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
        catch (Exception)
        {
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
        catch (Exception)
        {
            TempData["ErrorMessage"] = "An error occurred while deleting the booking.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdowns(int? selectedEventId = null, int? selectedVenueId = null)
    {
        ViewBag.EventId = new SelectList(
            await _context.Events.ToListAsync(), "EventId", "EventName", selectedEventId);
        ViewBag.VenueId = new SelectList(
            await _context.Venues.ToListAsync(), "VenueId", "VenueName", selectedVenueId);
    }
}
