using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace EventEaseApp.Services;

public class ImageProcessingService : IImageProcessingService
{
    private static readonly string[] AllowedExtensions =
        { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    private static readonly string[] AllowedContentTypes =
        { "image/jpeg", "image/png", "image/webp", "image/gif" };

    private readonly ILogger<ImageProcessingService> _logger;
    private readonly long _maxIncomingBytes;
    private readonly int _maxDimension;
    private readonly int _jpegQuality;
    private readonly long _maxOutputBytes;

    public ImageProcessingService(IConfiguration config, ILogger<ImageProcessingService> logger)
    {
        _logger = logger;
        _maxIncomingBytes = config.GetValue<long?>("ImageProcessing:MaxIncomingBytes") ?? 20_971_520L;
        _maxDimension = config.GetValue<int?>("ImageProcessing:MaxDimension") ?? 1920;
        _jpegQuality = config.GetValue<int?>("ImageProcessing:JpegQuality") ?? 84;
        _maxOutputBytes = config.GetValue<long?>("ImageProcessing:MaxOutputBytes") ?? 2_621_440L;
    }

    public async Task<ProcessedImage> ProcessAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
            throw new InvalidImageUploadException("No file was uploaded.");

        if (file.Length > _maxIncomingBytes)
            throw new InvalidImageUploadException(
                $"File is too large. Maximum allowed is {_maxIncomingBytes / (1024 * 1024)} MB.");

        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;
        if (!AllowedExtensions.Contains(extension))
            throw new InvalidImageUploadException(
                "Unsupported file extension. Use JPG, PNG, WEBP, or GIF.");

        var contentType = (file.ContentType ?? string.Empty).ToLowerInvariant();
        if (!AllowedContentTypes.Contains(contentType))
            throw new InvalidImageUploadException(
                "Unsupported content type. Use JPG, PNG, WEBP, or GIF.");

        Image image;
        IImageFormat format;
        try
        {
            using var buffer = new MemoryStream();
            await using (var input = file.OpenReadStream())
            {
                await input.CopyToAsync(buffer, cancellationToken);
            }
            buffer.Position = 0;
            format = await Image.DetectFormatAsync(buffer, cancellationToken);
            buffer.Position = 0;
            image = await Image.LoadAsync(buffer, cancellationToken);
        }
        catch (UnknownImageFormatException ex)
        {
            _logger.LogWarning(ex, "Image upload rejected: unsupported format ({FileName})", file.FileName);
            throw new InvalidImageUploadException("The file is not a recognised image.");
        }
        catch (InvalidImageContentException ex)
        {
            _logger.LogWarning(ex, "Image upload rejected: corrupt content ({FileName})", file.FileName);
            throw new InvalidImageUploadException("The image is corrupt or unreadable.");
        }

        try
        {
            if (image.Width <= 0 || image.Height <= 0)
                throw new InvalidImageUploadException("Image has invalid dimensions.");

            if (image.Width > _maxDimension || image.Height > _maxDimension)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(_maxDimension, _maxDimension),
                    Sampler = KnownResamplers.Lanczos3
                }));
            }

            var keepAlpha = HasAlpha(image, format);

            var output = new MemoryStream();
            string outputContentType;
            string outputExtension;

            if (keepAlpha)
            {
                await image.SaveAsPngAsync(output, new PngEncoder
                {
                    CompressionLevel = PngCompressionLevel.BestCompression
                }, cancellationToken);
                outputContentType = "image/png";
                outputExtension = ".png";
            }
            else
            {
                await image.SaveAsJpegAsync(output, new JpegEncoder
                {
                    Quality = _jpegQuality
                }, cancellationToken);
                outputContentType = "image/jpeg";
                outputExtension = ".jpg";
            }

            if (output.Length > _maxOutputBytes)
            {
                output.Dispose();
                output = new MemoryStream();
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(_maxDimension / 2, _maxDimension / 2),
                    Sampler = KnownResamplers.Lanczos3
                }));
                await image.SaveAsJpegAsync(output, new JpegEncoder
                {
                    Quality = Math.Max(60, _jpegQuality - 15)
                }, cancellationToken);
                outputContentType = "image/jpeg";
                outputExtension = ".jpg";

                if (output.Length > _maxOutputBytes)
                {
                    output.Dispose();
                    throw new InvalidImageUploadException(
                        "Could not reduce image enough; please try a smaller file.");
                }
            }

            output.Position = 0;
            return new ProcessedImage(output, outputContentType, outputExtension);
        }
        finally
        {
            image.Dispose();
        }
    }

    private static bool HasAlpha(Image image, IImageFormat format)
    {
        if (format is PngFormat) return true;

        var pixelType = image.PixelType;
        return pixelType.AlphaRepresentation == PixelAlphaRepresentation.Associated
            || pixelType.AlphaRepresentation == PixelAlphaRepresentation.Unassociated;
    }
}
