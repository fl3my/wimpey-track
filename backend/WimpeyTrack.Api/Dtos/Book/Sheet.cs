namespace WimpeyTrack.Api.Dtos.Book;

public class Sheet
{
    public ICollection<Row> Rows { get; set; } = new  List<Row>();
}