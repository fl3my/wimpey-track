using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using WimpeyTrack.Api.Dtos;

namespace WimpeyTrack.Api.Services;

public class ImageProcessingService : IImageProcessingService
{
    public string CropImageToBase64(byte[] imageBytes, BoundingBox box, float paddingPercent = 0.05f)
    {
        // Load the image
        using var image = Image.Load(imageBytes);
        
        // Add padding/ Add padding
        int padX = (int)(box.Width * paddingPercent);
        int padY = (int)(box.Height * paddingPercent);

        int x = Math.Max(0, box.X - padX);
        int y = Math.Max(0, box.Y - padY);
        int width = Math.Min(image.Width - x, box.Width + padX * 2);
        int height = Math.Min(image.Height - y, box.Height + padY * 2);

        image.Mutate(ctx => ctx.Crop(new Rectangle(x, y, width, height)));
        
        using var ms = new MemoryStream();
        image.SaveAsJpeg(ms);
        
        return Convert.ToBase64String(ms.ToArray());
    }
}