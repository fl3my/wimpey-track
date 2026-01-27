using WimpeyTrack.Api.Dtos;

namespace WimpeyTrack.Api.Services;

public interface IImageProcessingService
{
    string CropImageToBase64(byte[] image, BoundingBox box, float paddingPercent);
}