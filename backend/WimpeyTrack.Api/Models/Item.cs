namespace WimpeyTrack.Api.Models;

public class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public double Cost { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int PurchaseId { get; set; }
    public Purchase Purchase { get; set; } = null!;
}