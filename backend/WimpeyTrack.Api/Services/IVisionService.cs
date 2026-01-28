using WimpeyTrack.Api.Dtos.Vision;

namespace WimpeyTrack.Api.Services;

public interface IVisionService
{
    Task<VisionReceiptDetectionResponse> DetectReceiptsAsync(
        IFormFile file,
         CancellationToken cancellationToken = default
    );
}