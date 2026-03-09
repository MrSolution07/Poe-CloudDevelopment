using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEaseApp.Models;

public class Booking
{
    [Key]
    [Display(Name = "Booking ID")]
    public int BookingId { get; set; }

    [Required(ErrorMessage = "Event is required.")]
    [Display(Name = "Event")]
    public int EventId { get; set; }

    [ForeignKey("EventId")]
    public Event? Event { get; set; }

    [Required(ErrorMessage = "Venue is required.")]
    [Display(Name = "Venue")]
    public int VenueId { get; set; }

    [ForeignKey("VenueId")]
    public Venue? Venue { get; set; }

    [Required(ErrorMessage = "Booking date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Booking Date")]
    public DateTime BookingDate { get; set; }
}
