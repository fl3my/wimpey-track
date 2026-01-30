namespace WimpeyTrack.Api.Dtos.Purchase;

public class UpdatePurchaseDto
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public string StoreName { get; set; } = string.Empty;
}