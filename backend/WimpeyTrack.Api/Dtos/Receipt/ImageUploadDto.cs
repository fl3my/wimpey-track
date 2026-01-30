using System.ComponentModel.DataAnnotations;

namespace WimpeyTrack.Api.Dtos.Receipt;

public class ImageUploadDto
{
    [Required]
    public IFormFile File { get; set; } = null!;
}