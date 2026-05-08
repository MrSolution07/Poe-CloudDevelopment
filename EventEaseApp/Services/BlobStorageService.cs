using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace EventEaseApp.Services;

public interface IBlobStorageService
{
    Task<string> UploadImageAsync(IFormFile file, string containerName);
    Task<string> UploadProcessedImageAsync(Stream content, string contentType, string extension, string containerName);
    Task DeleteImageAsync(string imageUrl, string containerName);
    bool IsConfigured { get; }
}

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient? _blobServiceClient;
    private readonly ILogger<BlobStorageService> _logger;

    public bool IsConfigured => _blobServiceClient != null;

    public BlobStorageService(IConfiguration configuration, ILogger<BlobStorageService> logger)
    {
        _logger = logger;

        // GetConnectionString() returns null (never "") so is safe with ??.
        // configuration["AzureBlobStorage:ConnectionString"] can return "" from appsettings.json,
        // so it must be guarded with IsNullOrEmpty to avoid blocking the ?? chain.
        var rawValue = configuration["AzureBlobStorage:ConnectionString"];
        var connectionString = configuration.GetConnectionString("AzureBlobStorage")
            ?? configuration.GetConnectionString("BlobStorage")
            ?? (string.IsNullOrEmpty(rawValue) ? null : rawValue);

        if (string.IsNullOrWhiteSpace(connectionString) || IsPlaceholder(connectionString))
        {
            // Service starts in a "disabled" state. IsConfigured stays false and
            // controllers route uploads to a friendly message instead of throwing.
            _logger.LogInformation(
                "Azure Blob Storage is not configured; image uploads are disabled in this environment.");
            return;
        }

        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    // Treat the appsettings.AzureExample.json template as "not configured" so a
    // student that copy-pastes the example without filling in real credentials
    // gets the clear "not configured" message rather than an Azure auth error.
    private static bool IsPlaceholder(string connectionString)
        => connectionString.Contains("YOUR-ACCOUNT", StringComparison.OrdinalIgnoreCase)
        || connectionString.Contains("YOUR-KEY", StringComparison.OrdinalIgnoreCase)
        || connectionString.Contains("YOUR-SERVER", StringComparison.OrdinalIgnoreCase);

    public async Task<string> UploadImageAsync(IFormFile file, string containerName)
    {
        if (_blobServiceClient == null)
            throw new InvalidOperationException(
                "Azure Blob Storage is not configured. Set ConnectionStrings:AzureBlobStorage (or AzureBlobStorage:ConnectionString) in configuration.");

        var containerClient = await GetOrCreateContainerAsync(containerName);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var blobClient = containerClient.GetBlobClient(fileName);

        await using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream, new BlobHttpHeaders
        {
            ContentType = file.ContentType
        });

        return blobClient.Uri.ToString();
    }

    public async Task<string> UploadProcessedImageAsync(
        Stream content, string contentType, string extension, string containerName)
    {
        if (_blobServiceClient == null)
            throw new InvalidOperationException(
                "Azure Blob Storage is not configured. Set ConnectionStrings:AzureBlobStorage (or AzureBlobStorage:ConnectionString) in configuration.");

        var containerClient = await GetOrCreateContainerAsync(containerName);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var blobClient = containerClient.GetBlobClient(fileName);

        if (content.CanSeek) content.Position = 0;
        await blobClient.UploadAsync(content, new BlobHttpHeaders
        {
            ContentType = contentType
        });

        return blobClient.Uri.ToString();
    }

    public async Task DeleteImageAsync(string imageUrl, string containerName)
    {
        if (_blobServiceClient == null || string.IsNullOrEmpty(imageUrl))
            return;

        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var uri = new Uri(imageUrl);
            var blobName = Path.GetFileName(uri.LocalPath);
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to delete blob {ImageUrl} in container {Container}",
                imageUrl, containerName);
        }
    }

    private async Task<BlobContainerClient> GetOrCreateContainerAsync(string containerName)
    {
        var containerClient = _blobServiceClient!.GetBlobContainerClient(containerName);
        try
        {
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
        }
        catch (Azure.RequestFailedException ex) when (ex.ErrorCode == "PublicAccessNotPermitted")
        {
            // Storage account has public access disabled at account level.
            // Fall back to a private container — enable "Allow Blob public access" in the
            // Azure Portal to make images publicly viewable via direct URL.
            _logger.LogWarning(
                "Public blob access not permitted for {Container}; created as private container.",
                containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);
        }
        return containerClient;
    }
}
