namespace WimpeyTrack.Api.Dtos.Book;

public class PurchaseRow
{
    public string ExpenseCode  { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string ExpenseDetail { get; set; } = string.Empty;
    public string ReasonForExpense { get; set; } = string.Empty;
    public string VatCode { get; set; } = string.Empty;
    public bool ReceiptAttached { get; set; }
    public double Cost { get; set; }
}