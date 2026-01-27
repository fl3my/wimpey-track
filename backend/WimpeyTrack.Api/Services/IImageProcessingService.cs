using WimpeyTrack.Api.Dtos;

namespace WimpeyTrack.Api.Services;

public interface IImageProcessingService
{
    string CropImageToBase64(byte[] image, BoundingBox box, float paddingPercent);
    Task<byte[]> CombineReceiptsAsync(List<byte[]> imageBytesList, int receiptsPerRow = 5);
    Task<Stream> ResizeForOcrAsync(IFormFile file, int maxWidth, int maxHeight);
}