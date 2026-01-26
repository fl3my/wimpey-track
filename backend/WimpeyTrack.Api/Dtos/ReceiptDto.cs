using WimpeyTrack.Api.Enums;

namespace WimpeyTrack.Api.Dtos;

public class ReceiptDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public ReceiptCategory Category { get; set; }
    public string ImagePath {get; set;} = string.Empty;
    public int? PurchaseId { get; set; }
}