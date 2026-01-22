namespace WimpeyTrack.Api.Models;

public class Purchase
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public ICollection<Item> Items { get; set; } = new List<Item>();
}