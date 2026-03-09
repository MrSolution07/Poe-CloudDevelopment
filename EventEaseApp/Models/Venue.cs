using System.ComponentModel.DataAnnotations;

namespace EventEaseApp.Models;

public class Venue
{
    [Key]
    public int VenueId { get; set; }

    [Required(ErrorMessage = "Venue name is required.")]
    [StringLength(200)]
    [Display(Name = "Venue Name")]
    public string VenueName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Location is required.")]
    [StringLength(500)]
    public string Location { get; set; } = string.Empty;

    [Required(ErrorMessage = "Capacity is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1.")]
    public int Capacity { get; set; }

    [StringLength(1000)]
    [Display(Name = "Image URL")]
    public string? ImageUrl { get; set; }

    [Display(Name = "Available")]
    public bool IsAvailable { get; set; } = true;

    public ICollection<Event> Events { get; set; } = new List<Event>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
