using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEaseApp.Models;

public class BookingRequest
{
    [Key]
    public int BookingRequestId { get; set; }

    [Required(ErrorMessage = "Your name is required.")]
    [StringLength(200)]
    [Display(Name = "Full Name")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(200)]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    [Display(Name = "Phone Number")]
    public string? Phone { get; set; }

    [Display(Name = "Preferred Venue")]
    public int? PreferredVenueId { get; set; }

    [ForeignKey("PreferredVenueId")]
    public Venue? PreferredVenue { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Preferred Date")]
    public DateTime? PreferredDate { get; set; }

    [Required(ErrorMessage = "Please describe your booking request.")]
    [StringLength(2000)]
    public string Message { get; set; } = string.Empty;

    [StringLength(20)]
    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
