namespace EventEaseApp.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public int? StatusCode { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public string? OriginalPath { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
