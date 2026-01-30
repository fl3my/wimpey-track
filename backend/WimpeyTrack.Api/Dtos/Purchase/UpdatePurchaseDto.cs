using System.ComponentModel.DataAnnotations;

namespace WimpeyTrack.Api.Dtos.Purchase;

public class UpdatePurchaseDto
{
    [Required]
    public int Id { get; set; }
    [Required]
    [Range(typeof(DateOnly), "2024-04-06", "2034-04-06")]
    public DateOnly Date { get; set; }
    [Required]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Store Name must be between 3 and 50 characters")]
    public string StoreName { get; set; } = string.Empty;
}