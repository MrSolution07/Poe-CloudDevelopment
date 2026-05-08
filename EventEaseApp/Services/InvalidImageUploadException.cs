namespace EventEaseApp.Services;

public class InvalidImageUploadException : Exception
{
    public InvalidImageUploadException(string message) : base(message) { }
    public InvalidImageUploadException(string message, Exception inner) : base(message, inner) { }
}
