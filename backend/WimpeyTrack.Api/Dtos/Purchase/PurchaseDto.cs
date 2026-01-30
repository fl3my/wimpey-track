using WimpeyTrack.Api.Dtos.Item;

namespace WimpeyTrack.Api.Dtos.Purchase;

public class PurchaseDto
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public ICollection<ItemDto> Items { get; set; } = new List<ItemDto>();
}