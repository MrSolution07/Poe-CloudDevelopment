namespace EventEaseApp.Data;

public sealed class DatabaseBootstrapResult
{
    public bool IsAvailable { get; init; }

    public string Message { get; init; } = string.Empty;

    public static DatabaseBootstrapResult Available() => new() { IsAvailable = true };

    public static DatabaseBootstrapResult Unavailable(string message) =>
        new() { IsAvailable = false, Message = message };
}
