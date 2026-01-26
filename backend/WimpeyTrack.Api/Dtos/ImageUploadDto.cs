using System.ComponentModel.DataAnnotations;

namespace WimpeyTrack.Api.Dtos;

public class ImageUploadDto
{
    [Required]
    public IFormFile? File { get; set; }
}