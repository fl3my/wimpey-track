using WimpeyTrack.Api.Enums;

namespace WimpeyTrack.Api.Dtos;

public class ReceiptData
{
    public string StoreName { get; set; }
    public DateOnly? TransactionDate { get; set; }
    public ReceiptCategory ReceiptCategory { get; set; }
    public List<ReceiptItem> ReceiptItems { get; set; } = new();
    public BoundingBox? BoundingBox { get; set; }
}