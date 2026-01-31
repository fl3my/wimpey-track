using System.ComponentModel.DataAnnotations;
using WimpeyTrack.Api.Enums;

namespace WimpeyTrack.Api.Dtos.Receipt;

public class CreateReceiptBase64Dto
{
    [Required]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 characters")]
    public string Name { get; set; } = string.Empty;
    [Required]
    [Range(typeof(DateOnly), "2024-04-06", "2034-04-06")]
    public DateOnly Date { get; set; }
    [Required]
    public string Base64Content { get; set; } = string.Empty;
}