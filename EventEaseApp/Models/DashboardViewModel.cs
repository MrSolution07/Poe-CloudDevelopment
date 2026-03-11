namespace EventEaseApp.Models;

public class DashboardViewModel
{
    public int TotalVenues { get; set; }
    public int TotalEvents { get; set; }
    public int TotalBookings { get; set; }
    public int AvailableVenues { get; set; }
    public int UpcomingEvents { get; set; }
    public int PendingRequests { get; set; }
    public string UserName { get; set; } = string.Empty;
    public List<Booking> RecentBookings { get; set; } = new();
    public List<Event> UpcomingEventsList { get; set; } = new();
}
