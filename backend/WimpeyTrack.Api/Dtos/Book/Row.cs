namespace WimpeyTrack.Api.Dtos.Book;

public class Row
{
    public string Date { get; set; } = string.Empty;
    public ICollection<string> Description { get; set; } = new List<string>();
    public int TotalMiles { get; set; }
}
