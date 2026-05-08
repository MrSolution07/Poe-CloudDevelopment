namespace EventEaseApp.Services;

public interface IImageProcessingService
{
    Task<ProcessedImage> ProcessAsync(IFormFile file, CancellationToken cancellationToken = default);
}

public sealed class ProcessedImage : IDisposable
{
    public Stream Content { get; }
    public string ContentType { get; }
    public string Extension { get; }

    public ProcessedImage(Stream content, string contentType, string extension)
    {
        Content = content;
        ContentType = contentType;
        Extension = extension;
    }

    public void Dispose() => Content.Dispose();
}
