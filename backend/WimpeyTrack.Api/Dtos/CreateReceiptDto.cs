using System.ComponentModel.DataAnnotations;
using WimpeyTrack.Api.Enums;


namespace WimpeyTrack.Api.Dtos;

public class CreateReceiptDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public DateOnly Date { get; set; }
    [Required]
    public ReceiptCategory Category { get; set; }

    [Required] public IFormFile File { get; set; } = null!;
}