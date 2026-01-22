namespace WimpeyTrack.Api.Dtos;

public class CreateItemDto
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public double Cost { get; set; }
    public string Reason { get; set; } = string.Empty;
}