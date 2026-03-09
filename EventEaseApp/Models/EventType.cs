using System.ComponentModel.DataAnnotations;

namespace EventEaseApp.Models;

public class EventType
{
    [Key]
    public int EventTypeId { get; set; }

    [Required(ErrorMessage = "Event type name is required.")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
