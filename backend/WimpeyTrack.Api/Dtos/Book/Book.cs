namespace WimpeyTrack.Api.Dtos.Book;

public class Book
{
    public ICollection<Sheet> Sheets { get; set; } = new List<Sheet>();
    public ICollection<PurchaseRow> PurchaseRows { get; set; } = new List<PurchaseRow>();
    public bool IsOverThreshold { get; set; }
}