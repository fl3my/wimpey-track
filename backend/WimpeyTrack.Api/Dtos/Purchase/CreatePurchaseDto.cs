using WimpeyTrack.Api.Dtos.Item;

namespace WimpeyTrack.Api.Dtos.Purchase;

public class CreatePurchaseDto
{
    public DateOnly Date { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public int? ReceiptId { get; set; }
    public IList<CreateItemDto> Items { get; set; } = new List<CreateItemDto>();
}