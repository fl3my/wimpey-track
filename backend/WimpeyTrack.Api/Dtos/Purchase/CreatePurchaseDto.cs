using System.ComponentModel.DataAnnotations;
using WimpeyTrack.Api.Dtos.Item;

namespace WimpeyTrack.Api.Dtos.Purchase;

public class CreatePurchaseDto
{
    [Required]
    [Range(typeof(DateOnly), "2024-04-06", "2034-04-06")]
    public DateOnly Date { get; set; }
    
    [Required]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Store Name must be between 3 and 50 characters")]
    public string StoreName { get; set; } = string.Empty;
    public int? ReceiptId { get; set; }
    [Required]
    [MinLength(1, ErrorMessage = "At least one item is required")]
    public IList<CreateItemDto> Items { get; set; } = new List<CreateItemDto>();
}