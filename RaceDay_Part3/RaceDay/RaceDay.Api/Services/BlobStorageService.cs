using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Part2_Raceday_API.Services;

public class BlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _eventBannersContainer = "event-banners";
    private readonly string _profilePicturesContainer = "profile-pictures";

    public BlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AzureBlobStorage")
            ?? throw new InvalidOperationException("AzureBlobStorage connection string is missing.");
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task<string> UploadEventBannerAsync(IFormFile file, int eventId)
    {
        return await UploadFileAsync(file, _eventBannersContainer, $"event-{eventId}-{Guid.NewGuid()}{Path.GetExtension(file.FileName)}");
    }

    public async Task<string> UploadProfilePictureAsync(IFormFile file, int userId)
    {
        return await UploadFileAsync(file, _profilePicturesContainer, $"user-{userId}-{Guid.NewGuid()}{Path.GetExtension(file.FileName)}");
    }

    private async Task<string> UploadFileAsync(IFormFile file, string containerName, string blobName)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty.");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            throw new ArgumentException("Only image files (jpg, png, gif, webp) are allowed.");

        if (file.Length > 5 * 1024 * 1024) // 5 MB limit
            throw new ArgumentException("File size must be less than 5 MB.");

        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var blobClient = containerClient.GetBlobClient(blobName);

        var headers = new BlobHttpHeaders
        {
            ContentType = file.ContentType
        };

        using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = headers });

        return blobClient.Uri.ToString();
    }
}
