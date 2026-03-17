using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace EventEaseApp.Services;

public interface IBlobStorageService
{
    Task<string> UploadImageAsync(IFormFile file, string containerName);
    Task DeleteImageAsync(string imageUrl, string containerName);
    bool IsConfigured { get; }
}

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient? _blobServiceClient;

    public bool IsConfigured => _blobServiceClient != null;

    public BlobStorageService(IConfiguration configuration)
    {
        // GetConnectionString() returns null (never "") so is safe with ??.
        // configuration["AzureBlobStorage:ConnectionString"] can return "" from appsettings.json,
        // so it must be guarded with IsNullOrEmpty to avoid blocking the ?? chain.
        var v1 = configuration["AzureBlobStorage:ConnectionString"];
        var connectionString = configuration.GetConnectionString("AzureBlobStorage")
            ?? configuration.GetConnectionString("BlobStorage")
            ?? (string.IsNullOrEmpty(v1) ? null : v1);
        if (!string.IsNullOrEmpty(connectionString))
            _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task<string> UploadImageAsync(IFormFile file, string containerName)
    {
        if (_blobServiceClient == null)
            throw new InvalidOperationException(
                "Azure Blob Storage is not configured. Set ConnectionStrings:AzureBlobStorage (or AzureBlobStorage:ConnectionString) in configuration.");

        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        try
        {
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
        }
        catch (Azure.RequestFailedException ex) when (ex.ErrorCode == "PublicAccessNotPermitted")
        {
            // Storage account has public access disabled at account level.
            // Create as private container — enable "Allow Blob public access" in Azure Portal
            // on the storage account to make images publicly viewable via direct URL.
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);
        }

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

        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var uri = new Uri(imageUrl);
            var blobName = Path.GetFileName(uri.LocalPath);
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }
        catch (Exception)
        {
            // Image might have been deleted externally or URL is not a blob URL
        }
    }
}
