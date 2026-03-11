namespace EventEaseApp.Models;

public class BookingDetailViewModel
{
    public int BookingId { get; set; }
    public DateTime BookingDate { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string? Description { get; set; }
    public string? EventImageUrl { get; set; }
    public string? EventTypeName { get; set; }
    public int VenueId { get; set; }
    public string VenueName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public int Capacity { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsAvailable { get; set; }
}
