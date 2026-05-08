namespace EventEaseApp.Models;

// Keyless entity mapped to the SQL view vw_BookingDetail (see Database/Schema.sql).
// Used by the consolidated BookingsController.Overview action when a relational
// provider is in use. The InMemory provider falls back to a LINQ join with
// equivalent columns.
public class BookingDetailView
{
    public int BookingId { get; set; }
    public DateTime BookingDate { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string? EventDescription { get; set; }
    public string? EventImageUrl { get; set; }
    public string? EventTypeName { get; set; }
    public int VenueId { get; set; }
    public string VenueName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public int Capacity { get; set; }
    public string? VenueImageUrl { get; set; }
    public bool IsAvailable { get; set; }
}
