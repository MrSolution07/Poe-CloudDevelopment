using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace EventEaseApp.Services;

public interface IBlobStorageService
{
    Task<string> UploadImageAsync(IFormFile file, string containerName);
    Task DeleteImageAsync(string imageUrl, string containerName);
}

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration["AzureBlobStorage:ConnectionString"];
        if (!string.IsNullOrEmpty(connectionString))
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
        }
        else
        {
            _blobServiceClient = null!;
        }
    }

    public async Task<string> UploadImageAsync(IFormFile file, string containerName)
    {
        if (_blobServiceClient == null)
            throw new InvalidOperationException("Azure Blob Storage is not configured.");

        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var blobClient = containerClient.GetBlobClient(fileName);

        using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream, new BlobHttpHeaders
        {
            ContentType = file.ContentType
        });

        return blobClient.Uri.ToString();
    }

    public async Task DeleteImageAsync(string imageUrl, string containerName)
    {
        if (_blobServiceClient == null || string.IsNullOrEmpty(imageUrl))
            return;

        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var uri = new Uri(imageUrl);
        var blobName = Path.GetFileName(uri.LocalPath);
        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.DeleteIfExistsAsync();
    }
}
