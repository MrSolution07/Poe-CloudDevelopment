using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEaseApp.Models;

public class Event
{
    [Key]
    public int EventId { get; set; }

    [Required(ErrorMessage = "Event name is required.")]
    [StringLength(200)]
    [Display(Name = "Event Name")]
    public string EventName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Event date is required.")]
    [DataType(DataType.DateTime)]
    [Display(Name = "Event Date")]
    public DateTime EventDate { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [Display(Name = "Venue")]
    public int? VenueId { get; set; }

    [ForeignKey("VenueId")]
    public Venue? Venue { get; set; }

    [Display(Name = "Event Type")]
    public int? EventTypeId { get; set; }

    [ForeignKey("EventTypeId")]
    [Display(Name = "Event Type")]
    public EventType? EventType { get; set; }

    [StringLength(1000)]
    [Display(Name = "Image URL")]
    public string? ImageUrl { get; set; }

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
