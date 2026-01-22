namespace WimpeyTrack.Api.Dtos;

public class CreatePurchaseDto
{
    public DateOnly Date { get; set; }
    public string StoreName { get; set; } = string.Empty;
}