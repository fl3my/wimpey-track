using WimpeyTrack.Api.Dtos.Shared;

namespace WimpeyTrack.Api.Services;

public interface IImageProcessingService
{
    public Task<byte[]> CropAsync(Stream imageStream, BoundingBox box, CancellationToken cancellationToken = default);
    string CropImageToBase64(byte[] image, BoundingBox box, float paddingPercent);
    Task<byte[]> CombineReceiptsAsync(List<byte[]> imageBytesList, int receiptsPerRow = 5);
    Task<Stream> ResizeAsync(IFormFile file, long maxBytes = 4 * 1024 * 1024);
    Task<byte[]> ResizeAsync(byte[] imageBytes, long maxBytes = 4 * 1024 * 1024);
    
}