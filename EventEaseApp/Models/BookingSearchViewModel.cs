namespace EventEaseApp.Models;

public class BookingSearchViewModel
{
    public string? SearchTerm { get; set; }
    public int? EventTypeId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public bool? IsAvailable { get; set; }

    public List<BookingDetailViewModel> Results { get; set; } = new();
    public List<EventType> EventTypes { get; set; } = new();
}
