using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
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

    public async Task<byte[]> CombineReceiptsAsync(List<byte[]> imageBytesList, int receiptsPerRow = 5)
    {
        if (imageBytesList.Count == 0)
        {
            throw new ArgumentException("No images provided to combine");
        }

        // First pass to calculate average width
        var totalWidth = 0;
    
        foreach (var imageBytes in imageBytesList)
        {
            using var imageStream = new MemoryStream(imageBytes);
            using var image = await Image.LoadAsync(imageStream);
            totalWidth += image.Width;
        }
    
        var targetWidth = totalWidth / imageBytesList.Count;
        
        var resizedImages = new List<Image<Rgba32>>();
        var maxHeight = 0;

        try
        {
            foreach (var imageBytes in imageBytesList)
            {
                using var imageStream = new MemoryStream(imageBytes);
                var image = await Image.LoadAsync<Rgba32>(imageStream);

                // Calculate the height maintaining the aspect ratio
                var aspectRatio = (double)image.Height / image.Width;
                var targetHeight = (int)(targetWidth * aspectRatio);

                // Resize to target width
                image.Mutate(x => x.Resize(targetWidth, targetHeight));
                maxHeight = Math.Max(maxHeight, targetHeight);
                resizedImages.Add(image);
            }

            // Create the combined image
            var combinedWidth = targetWidth * Math.Min(imageBytesList.Count, receiptsPerRow);
            var combinedImage = new Image<Rgba32>(combinedWidth, maxHeight);

            // Fill with beige background
            combinedImage.Mutate(x => x.BackgroundColor(Color.Beige));

            // Draw each image top aligned
            for (var i = 0; i < resizedImages.Count; i++)
            {
                var currentImage = resizedImages[i];
                var xPosition = i * targetWidth;
                combinedImage.Mutate(ctx => ctx.DrawImage(currentImage, new Point(xPosition, 0), 1f));
            }

            // Write to memory stream and save
            using var outputStream = new MemoryStream();
            await combinedImage.SaveAsJpegAsync(outputStream);
            return outputStream.ToArray();
        }
        finally
        {
            foreach (var image in resizedImages)
            {
                image.Dispose();
            }
        }
    }
    
    public async Task<Stream> ResizeForOcrAsync(IFormFile file, int maxWidth, int maxHeight)
    {
        var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        using var image = await Image.LoadAsync(memoryStream);

        // Auto-rotate based on EXIF orientation
        image.Mutate(x => x.AutoOrient());
        
        // Resize while keeping aspect ratio
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Mode = ResizeMode.Max,
            Size = new Size(maxWidth, maxHeight)
        }));

        var outputStream = new MemoryStream();
        await image.SaveAsJpegAsync(outputStream, new JpegEncoder()
        {
            Quality = 85 // adjust for compression if needed
        });
        outputStream.Position = 0;

        return outputStream;
    }
}