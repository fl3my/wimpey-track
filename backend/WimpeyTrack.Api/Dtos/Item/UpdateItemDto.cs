namespace WimpeyTrack.Api.Dtos.Item;

public class UpdateItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public double Cost { get; set; }
    public string Reason { get; set; } = string.Empty;
}