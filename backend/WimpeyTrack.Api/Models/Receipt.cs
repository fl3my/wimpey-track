using WimpeyTrack.Api.Enums;

namespace WimpeyTrack.Api.Models;

public class Receipt
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ReceiptCategory Category { get; set; }
    public DateOnly Date { get; set; }
    public string ImagePath {get; set;} = string.Empty;
    public Purchase? Purchase { get; set; }

}