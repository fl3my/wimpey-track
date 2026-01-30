using System.ComponentModel.DataAnnotations;
using WimpeyTrack.Api.Enums;

namespace WimpeyTrack.Api.Dtos.Receipt;

public class CreateReceiptBase64Dto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public DateOnly Date { get; set; }
    [Required]
    public ReceiptCategory Category { get; set; }
    [Required]
    public string Base64Content { get; set; } = string.Empty;
}